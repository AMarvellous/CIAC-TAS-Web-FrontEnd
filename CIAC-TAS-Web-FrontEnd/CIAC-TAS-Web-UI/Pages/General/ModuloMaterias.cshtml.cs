using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.Helper;
using CIAC_TAS_Web_UI.ModelViews.General;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Refit;

namespace CIAC_TAS_Web_UI.Pages.General
{
    public class ModuloMateriasModel : PageModel
    {
        [BindProperty]
        public IEnumerable<ModuloMateriaModelView> ModuloMateriaModelView { get; set; } = new List<ModuloMateriaModelView>();

        [TempData]
        public string Message { get; set; }

        private readonly IConfiguration _configuration;
        public ModuloMateriasModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var moduloMateriaServiceApi = GetModuloMateriaServiceApi();
            var moduloMateriaResponse = await moduloMateriaServiceApi.GetAllAsync();

            if (!moduloMateriaResponse.IsSuccessStatusCode)
            {
                Message = "Ocurrio un error inesperado";

                return Page();
            }

            var moduloMaterias = moduloMateriaResponse.Content.Data;
            ModuloMateriaModelView = moduloMaterias.Select(x => new ModuloMateriaModelView
            {
                ModuloModelView = new ModuloModelView { Nombre = x.ModuloResponse.Nombre },
                MateriaModelView = new MateriaModelView { Nombre = x.MateriaResponse.Nombre }
            });

            return Page();
        }

        public IModuloMateriaServiceApi GetModuloMateriaServiceApi()
        {
            return RestService.For<IModuloMateriaServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
        }
    }
}
