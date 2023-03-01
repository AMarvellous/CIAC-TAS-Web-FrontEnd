using CIAC_TAS_Service.Contracts.V1.Requests;
using CIAC_TAS_Service.Contracts.V1.Responses;
using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.Helper;
using CIAC_TAS_Web_UI.ModelViews.ASA;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Refit;

namespace CIAC_TAS_Web_UI.Pages.ASA
{
    public class CuestionarioExamenAsaModel : PageModel
    {
        [BindProperty]
        public bool HasExamenProgramado { get; set; }

        [TempData]
        public string Message { get; set; }

        private readonly IConfiguration _configuration;
		private readonly ICuestionarioASAHelper _cuestionarioASAHelper;
		public CuestionarioExamenAsaModel(IConfiguration configuration, ICuestionarioASAHelper cuestionarioASAHelper)
        {
            _configuration = configuration;
			_cuestionarioASAHelper = cuestionarioASAHelper;
		}

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetString(Session.SessionUserId);
			var sessionToken = HttpContext.Session.GetString(Session.SessionToken);

			var userHasExamenProgramado = await _cuestionarioASAHelper.UserHasExamenProgramadoAsync(userId, sessionToken);

			if (userHasExamenProgramado.Item1)
            {
                Message = userHasExamenProgramado.Item2 == string.Empty ? null : userHasExamenProgramado.Item2;
				HasExamenProgramado = true;
            } else
            {
                HasExamenProgramado = false;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostNewCuestionarioExamenAsaAsync()
        {
            var userId = HttpContext.Session.GetString(Session.SessionUserId);
            var sessionToken = HttpContext.Session.GetString(Session.SessionToken);

            var userHasExamenProgramado = await _cuestionarioASAHelper.UserHasExamenProgramadoAsync(userId, sessionToken);

            if (userHasExamenProgramado.Item1)
            {
                Message = userHasExamenProgramado.Item2 == string.Empty ? null : userHasExamenProgramado.Item2;
                HasExamenProgramado = true;
            }
            else
            {
                HasExamenProgramado = false;
                
                return Page();
            }            

            //Check If has respuesta
            var respuestasAsaServiceApi = GetIRespuestasAsaServiceApi();
            var respuestasAsaResponse = await respuestasAsaServiceApi.GetAllByUserIdAsync(userId);

            if (!respuestasAsaResponse.IsSuccessStatusCode)
            {
                Message = "Ocurrio un error inesperado, vuelva a intentarlo";
                return Page();
            }

            var configuracionPreguntaAsa = await _cuestionarioASAHelper.ConfiguracionPreguntaAsaExamNowAsync(userId, sessionToken);

            if (configuracionPreguntaAsa == null)
            {
                Message = "Ocurrio un error inesperado, vuelva a intentarlo";

                return Page();
            }

            var respuestasAsaList = respuestasAsaResponse.Content.Data;

            // Checkear las respuestas temporales que tenemos
            if (respuestasAsaList.Count() > 0)
            {
                if (CheckRespuestasAsaArePracticasType(respuestasAsaList))
                {
                    //Si son practicas que se quedaron guardados en la tabla temporal, just delete/process it
                    await respuestasAsaServiceApi.ProcessRespuestasAsaAsync(userId);
                }
                else
                {
                    //Si son tipo examen, checkear si esas respuestas son de este examen o de uno anterior que pudo quedar colgado
                    var respuestaAsa = respuestasAsaList.First();

                    var fechaExamen = respuestaAsa.FechaEntrada;                    

                    var isPartOfExam = fechaExamen >= configuracionPreguntaAsa.FechaInicial && fechaExamen <= configuracionPreguntaAsa.FechaFin;
                    if (!isPartOfExam)
                    {
                        await respuestasAsaServiceApi.ProcessRespuestasAsaAsync(userId);
                    } else
                    {
                        //Son las preguntas temporales y todavia esta en horario de examen
                        return RedirectToPage("/ASA/ExamenAsa", "CuestionarioASAExamen");
                    }
                }
            }

            //Ya todo limpio en las respuestas, verificamos si es su primer examen y no esta intentando dar otro examen para la misma configuracion
            var respuestasAsaconsolidadoServiceApi = GetIRespuestasAsaconsolidadoServiceApi();
            var respuestasAsaconsolidadoResponse = await respuestasAsaconsolidadoServiceApi.UserHasAnswersInConsolidadoByConfiguracionId(userId, configuracionPreguntaAsa.Id);
            if (!respuestasAsaconsolidadoResponse.IsSuccessStatusCode)
            {
                Message = "Ocurrio un error inesperado, vuelva a intentarlo";

                return Page();
            }

            var userHasRespuestasInConsolidado = respuestasAsaconsolidadoResponse.Content;

            if (userHasRespuestasInConsolidado)
            {
                Message = "No se puede realizar un examen por segunda vez.";

                return Page();
            }



            // Ya con todo limpio, generar las preguntas
            var preguntaAsaServiceApi = GetIPreguntaAsaServiceApi();
            var preguntasRandomResponse = await preguntaAsaServiceApi.GetPreguntasRandomAsync(ICuestionarioASAHelper.NUMERO_PREGUNTAS_EXAMEN_DEFAULT, ICuestionarioASAHelper.PREGUNTA_INI_DEFAULT, ICuestionarioASAHelper.PREGUNTA_FIN_DEFAULT, new List<int>());

            if (!preguntasRandomResponse.IsSuccessStatusCode)
            {
                Message = "Ocurrio un error inesperado, vuelva a intentarlo";

                return Page();
            }

            var preguntaAsaRandomData = preguntasRandomResponse.Content.Data; //TODO: Que obtenga solo los IDs

            if (preguntaAsaRandomData.Count() <= 0)
            {
                Message = $"No se pudo generar un cuestionario, vuelva a intentarlo";                

                return Page();
            }

            var fechaEntrada = DateTime.Now;

            List<CreateRespuestasAsaRequest> createRespuestasAsaRequest = new List<CreateRespuestasAsaRequest>();
            foreach (var item in preguntaAsaRandomData)
            {
                createRespuestasAsaRequest.Add(new CreateRespuestasAsaRequest
                {
                    UserId = userId,
                    ConfiguracionId = configuracionPreguntaAsa.Id,
                    PreguntaAsaId = item.Id,
                    FechaEntrada = fechaEntrada,
                    EsExamen = true
                });
            }

            await respuestasAsaServiceApi.CreateBatchAsync(createRespuestasAsaRequest);

            return RedirectToPage("/ASA/ExamenAsa", "CuestionarioASAExamen");
        }

        private bool CheckRespuestasAsaArePracticasType(IEnumerable<RespuestasAsaResponse> respuestasAsaList)
        {            
            var respuestaAsa = respuestasAsaList.First();

            if (respuestaAsa.EsExamen)
            {
                return false;
            }

            return true;
        }

        private IRespuestasAsaServiceApi GetIRespuestasAsaServiceApi()
        {
            return RestService.For<IRespuestasAsaServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
        }

        private IPreguntaAsaServiceApi GetIPreguntaAsaServiceApi()
        {
            return RestService.For<IPreguntaAsaServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
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
