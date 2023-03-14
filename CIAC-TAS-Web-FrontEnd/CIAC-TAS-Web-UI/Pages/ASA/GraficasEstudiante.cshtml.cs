using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.Helper;
using CIAC_TAS_Web_UI.ModelViews.ASA;
using CIAC_TAS_Web_UI.ModelViews.Estudiante;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Refit;
using static CIAC_TAS_Service.Contracts.V1.ApiRoute;

namespace CIAC_TAS_Web_UI.Pages.ASA
{
    public class GraficasEstudianteModel : PageModel
    {
        [BindProperty]
        public IEnumerable<GraficasEstudianteListaModelView> GraficasEstudianteListaModelView { get; set; } = new List<GraficasEstudianteListaModelView>();

        [BindProperty]
        public string UserIdWhenAdmin { get; set; } = string.Empty;

        [TempData]
        public string Message { get; set; }

        private readonly IConfiguration _configuration;
        public GraficasEstudianteModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> OnGetAsync(string userId = null)
        {
            var respuestasAsaConsolidadoServiceApi = RestService.For<IRespuestasAsaconsolidadoServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });

            var filterByExamen = false;
            if (userId == null || userId == string.Empty)
            {
                userId = HttpContext.Session.GetString(Session.SessionUserId);
            }
            else
            {
                filterByExamen = true;
                UserIdWhenAdmin = userId;
            }
            
			var respuestasAsaConsolidadoResponse = await respuestasAsaConsolidadoServiceApi.GetAllHeadersByUserId(userId);

            if (!respuestasAsaConsolidadoResponse.IsSuccessStatusCode)
            {
                Message = "Ocurrio un error inesperado";

                return Page();
            }

            var respuestasAsaConsolidadosLista = respuestasAsaConsolidadoResponse.Content.Data;

            if (filterByExamen)
            {
                respuestasAsaConsolidadosLista = respuestasAsaConsolidadosLista.Where(x => x.EsExamen == true);
            }

            GraficasEstudianteListaModelView = respuestasAsaConsolidadosLista.Select(x => new GraficasEstudianteListaModelView {
                LoteRespuestasId = x.LoteRespuestasId,
                UserId = x.UserId,
                FechaLote = x.FechaLote,
                EsExamen = x.EsExamen,
            }).OrderByDescending(x => x.FechaLote);

            return Page();
        }
    }
}
