using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.Helper;
using CIAC_TAS_Web_UI.ModelViews.ASA;
using CIAC_TAS_Web_UI.ModelViews.Estudiante;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Refit;

namespace CIAC_TAS_Web_UI.Pages.ASA
{
    public class ReporteGraficaExamenModel : PageModel
    {
        [BindProperty]
        public IEnumerable<EstudianteRepoteGraficaExamenModelView> EstudianteRepoteGraficaExamenModelView { get; set; } = new List<EstudianteRepoteGraficaExamenModelView>();

        [TempData]
        public string Message { get; set; }

        private readonly IConfiguration _configuration;
        public ReporteGraficaExamenModel(IConfiguration configuration)
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
            EstudianteRepoteGraficaExamenModelView = estudiantes.Select(x => new EstudianteRepoteGraficaExamenModelView
            {
                Id = x.Id,
                UserId = x.UserId,
                Nombre = x.Nombre + " " + x.ApellidoPaterno + " " + x.ApellidoMaterno,
            });

            return Page();
        }
    }
}
