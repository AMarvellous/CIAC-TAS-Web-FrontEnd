using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.Helper;
using CIAC_TAS_Web_UI.ModelViews.Estudiante;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Refit;

namespace CIAC_TAS_Web_UI.Pages.Estudiante
{
    public class EstudiantesModel : PageModel
    {
        [BindProperty]
        public IEnumerable<EstudianteModelView> EstudiantesModelView { get; set; } = new List<EstudianteModelView>();

        [TempData]
        public string Message { get; set; }

        private readonly IConfiguration _configuration;
        public EstudiantesModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var estudianteServiceApi = RestService.For<IEstudianteServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
            var estudianteResponse = await estudianteServiceApi.GetAllAsync();

            if (!estudianteResponse.IsSuccessStatusCode)
            {
                Message = "Ocurrio un error inesperado";

                return Page();
            }

            var estudiantes = estudianteResponse.Content.Data;
            EstudiantesModelView = estudiantes.Select(x => new EstudianteModelView
            {
                Id = x.Id,
                Nombre = x.Nombre + " " + x.ApellidoPaterno + " " + x.ApellidoMaterno,
                CarnetIdentidad = x.CarnetIdentidad,
                Email = x.Email,
                CodigoTas = x.CodigoTas
            });

            return Page();
        }
    }
}
