using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.Helper;
using CIAC_TAS_Web_UI.ModelViews.General;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Refit;

namespace CIAC_TAS_Web_UI.Pages.General
{
    public class AdministrativosModel : PageModel
    {
        [BindProperty]
        public IEnumerable<AdministrativoModelView> AdministrativoModelView { get; set; } = new List<AdministrativoModelView>();

        [TempData]
        public string Message { get; set; }

        private readonly IConfiguration _configuration;
        public AdministrativosModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var administrativoServiceApi = GetAdministrativoServiceApi();
            var administrativoResponse = await administrativoServiceApi.GetAllAsync();

            if (!administrativoResponse.IsSuccessStatusCode)
            {
                Message = "Ocurrio un error inesperado";

                return Page();
            }

            var administrativos = administrativoResponse.Content.Data;
            AdministrativoModelView = administrativos.Select(x => new AdministrativoModelView
            {
                Id = x.Id,
                Nombres = x.Nombres + " " + x.ApellidoPaterno + " " + x.ApellidoMaterno,
                LicenciaCarnetIdentidad = x.LicenciaCarnetIdentidad,
            });

            return Page();
        }

        public IAdministrativoServiceApi GetAdministrativoServiceApi()
        {
            return RestService.For<IAdministrativoServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
        }
    }
}
