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
    public class CuestionarioASAModel : PageModel
    {
        [BindProperty]
        public CuestionarioASAModelView CuestionarioASAModelView { get; set; }
        public List<SelectListItem> GrupoPreguntaAsaOptions { get; set; }
        

        [TempData]
        public string Message { get; set; }

        private readonly IConfiguration _configuration;
        private readonly ICuestionarioASAHelper _cuestionarioASAHelper;
        public CuestionarioASAModel(IConfiguration configuration, ICuestionarioASAHelper cuestionarioASAHelper)
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
                CuestionarioASAModelView = new CuestionarioASAModelView();
                CuestionarioASAModelView.HasExamenProgramado = true;

                Message = "Hay un examen Programado para este usuario";

                return Page();
            } else
            {
				CuestionarioASAModelView = new CuestionarioASAModelView();
				CuestionarioASAModelView.HasExamenProgramado = false;
			}

            var respuestasAsaServiceApi = GetIRespuestasAsaServiceApi();
            var respuestasAsaHasRespuestasResponse = await respuestasAsaServiceApi.GetUserIdHasRespuestasAsaAsync(userId);

            if (!respuestasAsaHasRespuestasResponse.IsSuccessStatusCode)
            {
                Message = "Ocurrio un error inesperado, vuela a intentar cargar la pagina";

                return Page();
            }

            var grupoPreguntaAsaServiceApi = GetIGrupoPreguntaAsaServiceApi();
            var grupoPreguntaAsaResponse = await grupoPreguntaAsaServiceApi.GetAllAsync();

            if (grupoPreguntaAsaResponse.IsSuccessStatusCode)
            {
                GrupoPreguntaAsaOptions = grupoPreguntaAsaResponse.Content.Data.Select(x => new SelectListItem { Text = x.Nombre, Value = x.Id.ToString() }).ToList();
            }

            var hasRespuestas = respuestasAsaHasRespuestasResponse.Content;
            if (!hasRespuestas) //If it's new Quiz, show filters in UI
            {
                CuestionarioASAModelView.HasQuizInProgress = false;

                return Page();
            }

            CuestionarioASAModelView.HasQuizInProgress = true;
            //TODO: bloquear campos

            return Page();
        }

        public async Task<IActionResult> OnPostNewCuestionarioAsaAsync()
        {
            if (CuestionarioASAModelView.NumeroPreguntas == null)
            {
                CuestionarioASAModelView.NumeroPreguntas = ICuestionarioASAHelper.NUMERO_PREGUNTAS_DEFAULT;
            }

            if (CuestionarioASAModelView.PreguntaIni == null)
            {
                CuestionarioASAModelView.PreguntaIni = ICuestionarioASAHelper.PREGUNTA_INI_DEFAULT;
            }

            if (CuestionarioASAModelView.PreguntaFin == null)
            {
                CuestionarioASAModelView.PreguntaFin = ICuestionarioASAHelper.PREGUNTA_FIN_DEFAULT;
            }

            if (CuestionarioASAModelView.GrupoPreguntaAsaIds == null)
            {
                CuestionarioASAModelView.GrupoPreguntaAsaIds = new List<int>();
            }

            if (CuestionarioASAModelView.NumeroPreguntas <= 0 || CuestionarioASAModelView.NumeroPreguntas > 100)
            {
				Message = "El numero de preguntas debe ser minimo 1 y maximo 100";
				var grupoPreguntaAsaServiceApi = GetIGrupoPreguntaAsaServiceApi();
				var grupoPreguntaAsaResponse = await grupoPreguntaAsaServiceApi.GetAllAsync();

				if (grupoPreguntaAsaResponse.IsSuccessStatusCode)
				{
					GrupoPreguntaAsaOptions = grupoPreguntaAsaResponse.Content.Data.Select(x => new SelectListItem { Text = x.Nombre, Value = x.Id.ToString() }).ToList();
				}

				CuestionarioASAModelView.HasQuizInProgress = false;

				return Page();
			}

			if (CuestionarioASAModelView.PreguntaIni >= CuestionarioASAModelView.PreguntaFin && (CuestionarioASAModelView.PreguntaIni != 0 && CuestionarioASAModelView.PreguntaFin != 0))
			{
				Message = "El rango de preguntas iniciales debe ser menor al rango de preguntas finales";
				var grupoPreguntaAsaServiceApi = GetIGrupoPreguntaAsaServiceApi();
				var grupoPreguntaAsaResponse = await grupoPreguntaAsaServiceApi.GetAllAsync();

				if (grupoPreguntaAsaResponse.IsSuccessStatusCode)
				{
					GrupoPreguntaAsaOptions = grupoPreguntaAsaResponse.Content.Data.Select(x => new SelectListItem { Text = x.Nombre, Value = x.Id.ToString() }).ToList();
				}

				CuestionarioASAModelView.HasQuizInProgress = false;

				return Page();
			}

			if (CuestionarioASAModelView.PreguntaIni > ICuestionarioASAHelper.NUMERO_PREGUNTA_MAXIMA)
			{
				Message = $"El rango de maximo es {ICuestionarioASAHelper.NUMERO_PREGUNTA_MAXIMA}";
				var grupoPreguntaAsaServiceApi = GetIGrupoPreguntaAsaServiceApi();
				var grupoPreguntaAsaResponse = await grupoPreguntaAsaServiceApi.GetAllAsync();

				if (grupoPreguntaAsaResponse.IsSuccessStatusCode)
				{
					GrupoPreguntaAsaOptions = grupoPreguntaAsaResponse.Content.Data.Select(x => new SelectListItem { Text = x.Nombre, Value = x.Id.ToString() }).ToList();
				}

				CuestionarioASAModelView.HasQuizInProgress = false;

				return Page();
			}

			if (CuestionarioASAModelView.PreguntaIni < 0 ||
				CuestionarioASAModelView.PreguntaFin < 0)
			{
				Message = $"Los rangos de preguntas no pueden ser negativos";
				var grupoPreguntaAsaServiceApi = GetIGrupoPreguntaAsaServiceApi();
				var grupoPreguntaAsaResponse = await grupoPreguntaAsaServiceApi.GetAllAsync();

				if (grupoPreguntaAsaResponse.IsSuccessStatusCode)
				{
					GrupoPreguntaAsaOptions = grupoPreguntaAsaResponse.Content.Data.Select(x => new SelectListItem { Text = x.Nombre, Value = x.Id.ToString() }).ToList();
				}

				CuestionarioASAModelView.HasQuizInProgress = false;

				return Page();
			}

			var preguntaAsaServiceApi = GetIPreguntaAsaServiceApi();
            var preguntasRandomResponse = await preguntaAsaServiceApi.GetPreguntasRandomAsync((int)CuestionarioASAModelView.NumeroPreguntas, (int)CuestionarioASAModelView.PreguntaIni, (int)CuestionarioASAModelView.PreguntaFin, CuestionarioASAModelView.GrupoPreguntaAsaIds);

            if (!preguntasRandomResponse.IsSuccessStatusCode)
            {
                Message = "Ocurrio un error inesperado";
                var grupoPreguntaAsaServiceApi = GetIGrupoPreguntaAsaServiceApi();
                var grupoPreguntaAsaResponse = await grupoPreguntaAsaServiceApi.GetAllAsync();

                if (grupoPreguntaAsaResponse.IsSuccessStatusCode)
                {
                    GrupoPreguntaAsaOptions = grupoPreguntaAsaResponse.Content.Data.Select(x => new SelectListItem { Text = x.Nombre, Value = x.Id.ToString() }).ToList();
                }

				CuestionarioASAModelView.HasQuizInProgress = false;

				return Page();
            }

            var preguntaAsaRandomData = preguntasRandomResponse.Content.Data; //TODO: Que obtenga solo los IDs

            if (preguntaAsaRandomData.Count() <= 0)
            {
                Message = $"No se pudo generar un cuestionario con los datos configurados";
                var grupoPreguntaAsaServiceApi = GetIGrupoPreguntaAsaServiceApi();
                var grupoPreguntaAsaResponse = await grupoPreguntaAsaServiceApi.GetAllAsync();

                if (grupoPreguntaAsaResponse.IsSuccessStatusCode)
                {
                    GrupoPreguntaAsaOptions = grupoPreguntaAsaResponse.Content.Data.Select(x => new SelectListItem { Text = x.Nombre, Value = x.Id.ToString() }).ToList();
                }

                CuestionarioASAModelView.HasQuizInProgress = false;

                return Page();
            }

            var userId = HttpContext.Session.GetString(Session.SessionUserId);
            var fechaEntrada = DateTime.Now;

            List<CreateRespuestasAsaRequest> createRespuestasAsaRequest = new List<CreateRespuestasAsaRequest>();
            foreach (var item in preguntaAsaRandomData)
            {
                createRespuestasAsaRequest.Add(new CreateRespuestasAsaRequest
                {
                    UserId = userId,
                    PreguntaAsaId = item.Id,
                    FechaEntrada = fechaEntrada,
                    EsExamen = false
                });
            }

            var respuestasAsaServiceApi = GetIRespuestasAsaServiceApi();
            await respuestasAsaServiceApi.CreateBatchAsync(createRespuestasAsaRequest);
            
            return RedirectToPage("/ASA/CuestionarioASAPractica", "CuestionarioASAPractica");
        }

        private IGrupoPreguntaAsaServiceApi GetIGrupoPreguntaAsaServiceApi()
        {
            return RestService.For<IGrupoPreguntaAsaServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
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
    }
}
