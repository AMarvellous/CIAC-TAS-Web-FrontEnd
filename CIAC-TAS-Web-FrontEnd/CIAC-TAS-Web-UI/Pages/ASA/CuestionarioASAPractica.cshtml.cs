using CIAC_TAS_Service.Contracts.V1.Responses;
using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.Helper;
using CIAC_TAS_Web_UI.ModelViews.ASA;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Refit;

namespace CIAC_TAS_Web_UI.Pages.ASA
{
    public class CuestionarioASAPracticaModel : PageModel
    {
        [BindProperty]
        public ThumbnailViewModel ThumbnailViewModel { get; set; }

        [TempData]
        public string Message { get; set; }

        private readonly IConfiguration _configuration;

        public CuestionarioASAPracticaModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> OnGetCuestionarioASAPracticaAsync()
        {
            var userId = HttpContext.Session.GetString(Session.SessionUserId);
            var respuestasAsaServiceApi = GetIRespuestasAsaServiceApi();
            var respuestasAsaResponse = await respuestasAsaServiceApi.GetAllByUserIdAsync(userId);

            if (!respuestasAsaResponse.IsSuccessStatusCode)
            {
                Message = "Ocurrio un error";

                return RedirectToPage("/ASA/CuestionarioASA");
            }

            var respuestasAsaResponses = respuestasAsaResponse.Content.Data;

            //Check if this user has time left to do the quiz
			var tiempoRestante = await GetTiempoRestanteAsync(respuestasAsaResponses);

            if (tiempoRestante <= 0)
            {
                //In this case it doesn't matter the response
                var resp = await respuestasAsaServiceApi.ProcessRespuestasAsaAsync(userId);

                Message = "El cuestionario anterior ya finalizo, empiece otro cuestionario.";

				return RedirectToPage("/ASA/CuestionarioASA");
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

        public async Task<IActionResult> OnGetFinalizarCuestionarioASAPracticaAsync()
        {
			var userId = HttpContext.Session.GetString(Session.SessionUserId);
			var respuestasAsaServiceApi = GetIRespuestasAsaServiceApi();

			var resp = await respuestasAsaServiceApi.ProcessRespuestasAsaAsync(userId);

			return RedirectToPage("/ASA/CuestionarioASA");
        }


		public async Task<JsonResult> OnGetUpdateAnswerAsync(int opcionSeleccionadaId, int respuestasAsaId, string color)
		{
			var respuestasAsaServiceApi = GetIRespuestasAsaServiceApi();
			var respuestasAsaServiceResponse = await respuestasAsaServiceApi.PatchAsync(respuestasAsaId, new CIAC_TAS_Service.Contracts.V1.Requests.PatchRespuestasAsaRequest
			{
				OpcionSeleccionadaId = opcionSeleccionadaId,
                ColorInterfaz = color
			});

			if (!respuestasAsaServiceResponse.IsSuccessStatusCode)
			{
				return new JsonResult("Error al intentar actualizar la respuesta");
			}

			return new JsonResult("Actualizacion correcta");
		}

		public async Task<JsonResult> OnGetUpdateAnswerColorInterfazAsync(string color, int respuestasAsaId)
		{
			var respuestasAsaServiceApi = GetIRespuestasAsaServiceApi();
			var respuestasAsaServiceResponse = await respuestasAsaServiceApi.PatchAsync(respuestasAsaId, new CIAC_TAS_Service.Contracts.V1.Requests.PatchRespuestasAsaRequest
			{
				ColorInterfaz = color
			});

			if (!respuestasAsaServiceResponse.IsSuccessStatusCode)
			{
				return new JsonResult("Error al intentar actualizar la respuesta");
			}

			return new JsonResult("Actualizacion correcta");
		}

		private async Task<long> GetTiempoRestanteAsync(IEnumerable<RespuestasAsaResponse> respuestasAsa)
        {
            var tiempoLimite = Math.Ceiling(respuestasAsa.Count() * CuestionarioASAHelper.TIEMPO_POR_PREGUNTA_DEFAULT);

            var respuestaAsaFirstRow = respuestasAsa.FirstOrDefault();
            var fechaEntrada = respuestaAsaFirstRow.FechaEntrada;
			TimeSpan timeDifference = DateTime.Now - fechaEntrada;
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
    }
}
