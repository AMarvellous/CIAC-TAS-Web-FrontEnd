using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.Helper;
using CIAC_TAS_Web_UI.ModelViews.Estudiante;
using CIAC_TAS_Web_UI.ModelViews.General;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Refit;

namespace CIAC_TAS_Web_UI.Pages.Estudiante
{
    public class EstudiantesMateriasModel : PageModel
    {
        [BindProperty]
        public IEnumerable<GrupoModelView> GrupoModelView { get; set; } = new List<GrupoModelView>();
        
        [TempData]
        public string Message { get; set; }

        private readonly IConfiguration _configuration;
        public EstudiantesMateriasModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var grupoServiceApi = GetIGrupoServiceApiServiceApi();
            var grupoResponse = await grupoServiceApi.GetAllAsync();

            if (!grupoResponse.IsSuccessStatusCode)
            {
                Message = "Ocurrio un error inesperado";

                return RedirectToPage("/Index");
            }

            GrupoModelView = grupoResponse.Content.Data
                .Select(x => new GrupoModelView {
                    Id = x.Id,
                    Nombre = x.Nombre,
                });

            return Page();
        }

        private IGrupoServiceApi GetIGrupoServiceApiServiceApi()
        {
            return RestService.For<IGrupoServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
        }
    }
}
