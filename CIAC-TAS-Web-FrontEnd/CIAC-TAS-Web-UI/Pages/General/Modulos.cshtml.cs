using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.Helper;
using CIAC_TAS_Web_UI.ModelViews.General;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Refit;

namespace CIAC_TAS_Web_UI.Pages.General
{
    public class ModulosModel : PageModel
    {
        [BindProperty]
        public IEnumerable<ModuloModelView> ModuloModelView { get; set; } = new List<ModuloModelView>();

        [TempData]
        public string Message { get; set; }

        private readonly IConfiguration _configuration;
        public ModulosModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var moduloServiceApi = GetModuloServiceApi();
            var moduloResponse = await moduloServiceApi.GetAllAsync();

            if (!moduloResponse.IsSuccessStatusCode)
            {
                Message = "Ocurrio un error inesperado";

                return Page();
            }

            var Modulos = moduloResponse.Content.Data;
            ModuloModelView = Modulos.Select(x => new ModuloModelView
            {
                Id = x.Id,
                ModuloCodigo = x.ModuloCodigo,
                Nombre = x.Nombre,
            });

            return Page();
        }

        public IModuloServiceApi GetModuloServiceApi()
        {
            return RestService.For<IModuloServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
        }
    }
}
