using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Refit;

namespace CIAC_TAS_Web_UI.Pages.General
{
    public class CierreMateriaViewModel : PageModel
    {
        public List<SelectListItem> GrupoOptions { get; set; }
        public List<SelectListItem> MateriaOptions { get; set; }
        [BindProperty]
        public int GrupoId { get; set; }
        [BindProperty]
        public int MateriaId { get; set; }
        [TempData]
        public string Message { get; set; }

        private readonly IConfiguration _configuration;
        public CierreMateriaViewModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task OnGetAsync()
        {
            await FillGrupoOptions();
            await FillMateriaOptions();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (GrupoId == 0)
            {
                Message = "Seleccione un Grupo para continuar";

                await FillGrupoOptions();
                await FillMateriaOptions();
                
                return Page();
            }

            if (MateriaId == 0)
            {
                Message = "Seleccione una Materia para continuar";

                await FillGrupoOptions();
                await FillMateriaOptions();

                return Page();
            }

            return RedirectToPage("/General/CierreMateriaDetail", new { grupoId = GrupoId, materiaId = MateriaId });
        }

        private async Task FillGrupoOptions()
        {
            var grupoServiceApi = GetIGrupoServiceApi();
            var grupoResponse = await grupoServiceApi.GetAllAsync();

            if (grupoResponse.IsSuccessStatusCode)
            {
                GrupoOptions = grupoResponse.Content.Data.Select(x => new SelectListItem { Text = x.Nombre, Value = x.Id.ToString() }).ToList();
            }
        }

        private async Task FillMateriaOptions()
        {
            var materiaServiceApi = GetIMateriaServiceApi();
            var materiaResponse = await materiaServiceApi.GetAllAsync();

            if (materiaResponse.IsSuccessStatusCode)
            {
                MateriaOptions = materiaResponse.Content.Data.Select(x => new SelectListItem { Text = x.Nombre, Value = x.Id.ToString() }).ToList();
            }
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
    }
}
