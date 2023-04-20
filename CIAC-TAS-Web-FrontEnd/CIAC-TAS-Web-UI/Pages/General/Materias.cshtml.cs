using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.Helper;
using CIAC_TAS_Web_UI.ModelViews.General;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Refit;

namespace CIAC_TAS_Web_UI.Pages.General
{
    public class MateriasModel : PageModel
    {
        [BindProperty]
        public IEnumerable<MateriaModelView> MateriaModelView { get; set; } = new List<MateriaModelView>();

        [TempData]
        public string Message { get; set; }

        private readonly IConfiguration _configuration;
        public MateriasModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var materiaServiceApi = GetMateriaServiceApi();
            var materiaResponse = await materiaServiceApi.GetAllAsync();

            if (!materiaResponse.IsSuccessStatusCode)
            {
                Message = "Ocurrio un error inesperado";

                return Page();
            }

            var materias = materiaResponse.Content.Data;
            MateriaModelView = materias.Select(x => new MateriaModelView
            {
                Id = x.Id,
                Nombre = x.Nombre,
            });

            return Page();
        }

        public IMateriaServiceApi GetMateriaServiceApi()
        {
            return RestService.For<IMateriaServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
        }
    }
}
