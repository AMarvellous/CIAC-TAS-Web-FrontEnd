using CIAC_TAS_Service.Contracts.V1.Responses;
using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.Helper;
using CIAC_TAS_Web_UI.ModelViews.ASA;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Refit;

namespace CIAC_TAS_Web_UI.Pages.ASA
{
    public class ExamenAsaModel : PageModel
    {
		[BindProperty]
		public ThumbnailViewModel ThumbnailViewModel { get; set; }

		[TempData]
		public string Message { get; set; }
		private readonly IConfiguration _configuration;
		private readonly ICuestionarioASAHelper _cuestionarioASAHelper;
		public ExamenAsaModel(IConfiguration configuration, ICuestionarioASAHelper cuestionarioASAHelper)
		{
			_configuration = configuration;
			_cuestionarioASAHelper = cuestionarioASAHelper;
		}

		public async Task<IActionResult> OnGetCuestionarioASAExamenAsync()
		{
            var userId = HttpContext.Session.GetString(Session.SessionUserId);
            var sessionToken = HttpContext.Session.GetString(Session.SessionToken);

            var userHasExamenProgramado = await _cuestionarioASAHelper.UserHasExamenProgramadoAsync(userId, sessionToken);

            if (!userHasExamenProgramado.Item1)
            {
                Message = "El examen Programado para este usuario ya finalizo";

                return RedirectToPage("/ASA/CuestionarioExamenAsa");
            }

            var respuestasAsaServiceApi = GetIRespuestasAsaServiceApi();
            var respuestasAsaResponse = await respuestasAsaServiceApi.GetAllByUserIdAsync(userId);

            if (!respuestasAsaResponse.IsSuccessStatusCode)
            {
                Message = "Ocurrio un error inesperado, vuelva a intentarlo";

                return RedirectToPage("/ASA/CuestionarioExamenAsa");
            }

            var respuestasAsaResponses = respuestasAsaResponse.Content.Data;

            var configuracionPreguntaAsa = await _cuestionarioASAHelper.ConfiguracionPreguntaAsaExamNowAsync(userId, sessionToken);

            if (configuracionPreguntaAsa == null)
            {
                Message = "Ocurrio un error inesperado, vuelva a intentarlo";

                return RedirectToPage("/ASA/CuestionarioExamenAsa");
            }


            var respuestasAsaconsolidadoServiceApi = GetIRespuestasAsaconsolidadoServiceApi();
            var respuestasAsaconsolidadoResponse = await respuestasAsaconsolidadoServiceApi.UserHasAnswersInConsolidadoByConfiguracionId(userId, configuracionPreguntaAsa.Id);
            if (!respuestasAsaconsolidadoResponse.IsSuccessStatusCode)
            {
                Message = "Ocurrio un error inesperado, vuelva a intentarlo";

                return RedirectToPage("/ASA/CuestionarioExamenAsa");
            }

            var userHasRespuestasInConsolidado = respuestasAsaconsolidadoResponse.Content;

            if (userHasRespuestasInConsolidado)
            {
                Message = "No se puede realizar un examen por segunda vez.";

                return RedirectToPage("/ASA/CuestionarioExamenAsa");
            }


            //Check if this user has time left to do the quiz
            var tiempoRestante = await GetTiempoRestanteAsync(configuracionPreguntaAsa);

            if (tiempoRestante <= 0)
            {
                //In this case it doesn't matter the response
                var resp = await respuestasAsaServiceApi.ProcessRespuestasAsaAsync(userId);

                Message = "El tiempo del examen ya termino.";

                return RedirectToPage("/ASA/CuestionarioExamenAsa");
            }


            ThumbnailViewModel = new ThumbnailViewModel();
            ThumbnailViewModel.NumeroPreguntas = respuestasAsaResponses.Count();
            ThumbnailViewModel.RespuestasRespondidas = respuestasAsaResponses.Where(x => x.OpcionSeleccionadaId != null).Count();
            ThumbnailViewModel.RespuestasNoRespondidas = ThumbnailViewModel.NumeroPreguntas - ThumbnailViewModel.RespuestasRespondidas;
            ThumbnailViewModel.RespuestasNoSeguras = respuestasAsaResponses.Where(x => x.ColorInterfaz == "Warning").Count();
            ThumbnailViewModel.TiempoRestante = tiempoRestante;
            ThumbnailViewModel.ThumbnailModelList = new List<ThumbnailModel>();

            List<RespuestasAsaResponse> _detaisllist = new List<RespuestasAsaResponse>();

            foreach (var item in respuestasAsaResponses)
            {
                _detaisllist.Add(item);
            }

            var listOfBatches = _detaisllist.Batch(10);
            int tabNo = 1;

            foreach (var batchItem in listOfBatches)
            {
                // Generating tab
                ThumbnailModel obj = new ThumbnailModel();
                obj.ThumbnailLabel = "Lebel" + tabNo;
                obj.ThumbnailTabId = "tab" + tabNo;
                obj.ThumbnailTabNo = tabNo;
                obj.Thumbnail_Aria_Controls = "tab" + tabNo;
                obj.Thumbnail_Href = "#tab" + tabNo;

                // batch details
                obj.CuestionarioPreguntasAndRespuestasAsaView = new List<RespuestasAsaResponse>();

                foreach (var item in batchItem)
                {
                    RespuestasAsaResponse detailsObj = new RespuestasAsaResponse();
                    detailsObj = item;
                    obj.CuestionarioPreguntasAndRespuestasAsaView.Add(detailsObj);
                }

                ThumbnailViewModel.ThumbnailModelList.Add(obj);

                tabNo++;
            }

            // Getting first tab data
            var first = ThumbnailViewModel.ThumbnailModelList.FirstOrDefault();

            // Getting first tab data
            var last = ThumbnailViewModel.ThumbnailModelList.LastOrDefault();

            foreach (var item in ThumbnailViewModel.ThumbnailModelList)
            {
                if (item.ThumbnailTabNo == first.ThumbnailTabNo)
                {
                    item.Thumbnail_ItemPosition = "first";
                }

                if (item.ThumbnailTabNo == last.ThumbnailTabNo)
                {
                    item.Thumbnail_ItemPosition = "last";
                }
            }

            return Page();
        }

        private async Task<long> GetTiempoRestanteAsync(ConfiguracionPreguntaAsaResponse configuracionPreguntaAsa)
        {
            var tiempoLimite = ICuestionarioASAHelper.TIEMPO_LIMITE_EXAMEN_DEFAULT;

            //var respuestaAsaFirstRow = respuestasAsa.FirstOrDefault();
            //var fechaEntrada = respuestaAsaFirstRow.FechaEntrada;
            var horaInicioExamen = configuracionPreguntaAsa.FechaInicial;
            TimeSpan timeDifference = DateTime.Now - horaInicioExamen;
            var tiempoRestante = Convert.ToInt64(tiempoLimite) - timeDifference.TotalMinutes;

            return Convert.ToInt64(tiempoRestante);
        }

        private IRespuestasAsaServiceApi GetIRespuestasAsaServiceApi()
        {
            return RestService.For<IRespuestasAsaServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
        }
        private IRespuestasAsaconsolidadoServiceApi GetIRespuestasAsaconsolidadoServiceApi()
        {
            return RestService.For<IRespuestasAsaconsolidadoServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
        }
    }
}
