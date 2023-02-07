using CIAC_TAS_Service.Contracts.V1.Requests;
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
        public CuestionarioASAModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetString(Session.SessionUserId);
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
                return Page();
            }

            CuestionarioASAModelView = new CuestionarioASAModelView();
            CuestionarioASAModelView.HasQuizInProgress = true;
            //TODO: bloquear campos

            return Page();
        }

        public async Task<IActionResult> OnPostNewCuestionarioAsaAsync()
        {
            if (CuestionarioASAModelView.NumeroPreguntas == null)
            {
                CuestionarioASAModelView.NumeroPreguntas = CuestionarioASAHelper.NUMERO_PREGUNTAS_DEFAULT;
            }

            if (CuestionarioASAModelView.PreguntaIni == null)
            {
                CuestionarioASAModelView.PreguntaIni = CuestionarioASAHelper.PREGUNTA_INI_DEFAULT;
            }

            if (CuestionarioASAModelView.PreguntaFin == null)
            {
                CuestionarioASAModelView.PreguntaFin = CuestionarioASAHelper.PREGUNTA_FIN_DEFAULT;
            }

            if (CuestionarioASAModelView.GrupoPreguntaAsaIds == null)
            {
                CuestionarioASAModelView.GrupoPreguntaAsaIds = new List<int>();
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

                return Page();
            }

            var preguntaAsaRandomData = preguntasRandomResponse.Content.Data; //TODO: Que obtenga solo los IDs
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
