using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.Helper;
using CIAC_TAS_Web_UI.ModelViews.General;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Refit;
using static CIAC_TAS_Service.Contracts.V1.ApiRoute;

namespace CIAC_TAS_Web_UI.Pages.General
{
    public class CierreMateriaDetailModel : PageModel
    {
        [TempData]
        public string Message { get; set; }
        public MateriaModelView MateriaModelView { get; set; }
        public GrupoModelView GrupoModelView { get; set; }
        public CierreMateriaDetailModelView CierreMateriaDetailModelView { get; set; }

        private readonly IConfiguration _configuration;
        public CierreMateriaDetailModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> OnGetAsync(int grupoId, int materiaId)
        {
            if (grupoId == 0 || materiaId == 0)
            {
                return RedirectToPage("/General/CierreMateriaView");
            }

            var cierreMateriaServiceApi = GetICierreMateriaServiceApi();
            var cierreMateriaResponse = await cierreMateriaServiceApi.GetByGrupoIdMateriaIdAsync(grupoId, materiaId);

            if (!cierreMateriaResponse.IsSuccessStatusCode)
            {
                return RedirectToPage("/General/CierreMateriaView");
            }

            var grupoServiceApi = GetIGrupoServiceApi();
            var grupoResponse = await grupoServiceApi.GetAsync(grupoId);

            if (!grupoResponse.IsSuccessStatusCode)
            {
                return RedirectToPage("/General/CierreMateriaView");
            }

            GrupoModelView = new GrupoModelView
            { 
                Id = grupoResponse.Content.Id,
                Nombre = grupoResponse.Content.Nombre
            };

            var materiaServiceApi = GetIMateriaServiceApi();
            var materiaResponse = await materiaServiceApi.GetAsync(materiaId);

            if (!materiaResponse.IsSuccessStatusCode)
            {
                return RedirectToPage("/General/CierreMateriaView");
            }

            MateriaModelView = new MateriaModelView
            {
                Id = materiaResponse.Content.Id,
                Nombre = materiaResponse.Content.Nombre
            };

            if (cierreMateriaResponse.Content != null)
            {
                CierreMateriaDetailModelView = new CierreMateriaDetailModelView
                {
                    Id = cierreMateriaResponse.Content.Id,
                    GrupoId = cierreMateriaResponse.Content.GrupoId,
                    MateriaId = cierreMateriaResponse.Content.MateriaId
                };
            }

            return Page();
        }

        public async Task<IActionResult> OnGetCerrarMateriaAsync(int materiaId, int grupoId)
        {
            var cierreMateriaServiceApi = GetICierreMateriaServiceApi();
            var createRequest = new CIAC_TAS_Service.Contracts.V1.Requests.CreateCierreMateriaRequest
            {
                MateriaId = materiaId,
                GrupoId = grupoId,
            };
            var cierreMateriaResponse = await cierreMateriaServiceApi.CreateAsync(createRequest);

            if (!cierreMateriaResponse.IsSuccessStatusCode)
            {
                Message = "Ocurrio un error inesperado";

                return RedirectToPage("/General/CierreMateriaDetail", new { grupoId = grupoId, materiaId = materiaId });
            }

            return RedirectToPage("/General/CierreMateriaDetail", new { grupoId = grupoId, materiaId = materiaId });
        }

        public async Task<IActionResult> OnGetAbrirMateriaAsync(int materiaId, int grupoId, int cierreMateriaId)
        {
            var cierreMateriaServiceApi = GetICierreMateriaServiceApi();
            
            var cierreMateriaResponse = await cierreMateriaServiceApi.DeleteAsync(cierreMateriaId);

            if (!cierreMateriaResponse.IsSuccessStatusCode)
            {
                Message = "Ocurrio un error inesperado";

                return RedirectToPage("/General/CierreMateriaDetail", new { grupoId = grupoId, materiaId = materiaId });
            }

            return RedirectToPage("/General/CierreMateriaDetail", new { grupoId = grupoId, materiaId = materiaId });
        }

        private IGrupoServiceApi GetIGrupoServiceApi()
        {
            return RestService.For<IGrupoServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
        }

        private IMateriaServiceApi GetIMateriaServiceApi()
        {
            return RestService.For<IMateriaServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
        }

        private ICierreMateriaServiceApi GetICierreMateriaServiceApi()
        {
            return RestService.For<ICierreMateriaServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
        }
    }
}
