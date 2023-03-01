using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.Helper;
using CIAC_TAS_Web_UI.ModelViews.Estudiante;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Refit;

namespace CIAC_TAS_Web_UI.Pages.Estudiante
{
    public class EstudiantesGruposModel : PageModel
    {
        [BindProperty]
        public IEnumerable<EstudianteGrupoModelView> EstudianteGrupoModelView { get; set; } = new List<EstudianteGrupoModelView>();

        [TempData]
        public string Message { get; set; }

        private readonly IConfiguration _configuration;
        public EstudiantesGruposModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var estudianteGrupoServiceApi = RestService.For<IEstudianteGrupoServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
            var estudianteGrupoResponse = await estudianteGrupoServiceApi.GetAllAsync();

            if (!estudianteGrupoResponse.IsSuccessStatusCode)
            {
                Message = "Ocurrio un error inesperado";

                return Page();
            }

            var estudiantesGrupos = estudianteGrupoResponse.Content.Data;
            EstudianteGrupoModelView = estudiantesGrupos.Select(x => new EstudianteGrupoModelView
            {
                EstudianteNombre = x.EstudianteResponse.Nombre +" "+ x.EstudianteResponse.ApellidoPaterno + " "+ x.EstudianteResponse.ApellidoMaterno,
                GrupoNombre = x.GrupoResponse.Nombre
            });

            return Page();
        }
    }
}
