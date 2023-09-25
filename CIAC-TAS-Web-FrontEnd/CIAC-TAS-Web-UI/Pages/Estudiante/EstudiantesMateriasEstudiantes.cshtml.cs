using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.Helper;
using CIAC_TAS_Web_UI.ModelViews.Estudiante;
using CIAC_TAS_Web_UI.ModelViews.General;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Refit;

namespace CIAC_TAS_Web_UI.Pages.Estudiante
{
    public class EstudiantesMateriasEstudiantesModel : PageModel
    {
        [BindProperty]
        public IEnumerable<EstudianteGrupoModelView> EstudianteGrupoModelView { get; set; } = new List<EstudianteGrupoModelView>();
        public string NombreGrupo { get; set; } = string.Empty;

        [TempData]
        public string Message { get; set; }

        private readonly IConfiguration _configuration;
        public EstudiantesMateriasEstudiantesModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> OnGetAsync(int grupoId)
        {
            var grupoServiceApi = GetIGrupoServiceApi();
            var grupoResponse = await grupoServiceApi.GetAsync(grupoId);

            if (grupoResponse.IsSuccessStatusCode)
            {
                NombreGrupo = grupoResponse.Content.Nombre;
            }

            var estudianteGrupoServiceApi = GetIEstudianteGrupoServiceApi();
            var estudianteGrupoResponse = await estudianteGrupoServiceApi.GetAllByGrupoIdAsync(grupoId);

            if (!estudianteGrupoResponse.IsSuccessStatusCode)
            {
                Message = "Ocurrio un error inesperado";

                return RedirectToPage("/Estudiante/EstudiantesMaterias");
            }

            EstudianteGrupoModelView = estudianteGrupoResponse.Content.Data.Select(x => new EstudianteGrupoModelView
            {
                EstudianteId = x.EstudianteId,
                EstudianteNombre = x.EstudianteResponse.Nombre + " " + x.EstudianteResponse.ApellidoPaterno + " " + x.EstudianteResponse.ApellidoMaterno,
                GrupoId = x.GrupoId,
                GrupoNombre = x.GrupoResponse.Nombre 
            });

            return Page();
        }

        private IEstudianteGrupoServiceApi GetIEstudianteGrupoServiceApi()
        {
            return RestService.For<IEstudianteGrupoServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
        }

        private IGrupoServiceApi GetIGrupoServiceApi()
        {
            return RestService.For<IGrupoServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
        }
    }
}
