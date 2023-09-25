using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.Helper;
using CIAC_TAS_Web_UI.ModelViews.Estudiante;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Refit;
using static CIAC_TAS_Service.Contracts.V1.ApiRoute;

namespace CIAC_TAS_Web_UI.Pages.Estudiante
{
    public class EstudianteMateriaModel : PageModel
    {
        [BindProperty]
        public IEnumerable<EstudianteMateriaModelView> EstudianteMateriaModelView { get; set; } = new List<EstudianteMateriaModelView>();
        public string NombreGrupo { get; set; } = string.Empty;
        public string NombreEstudiante { get; set; } = string.Empty;
        public int EstudianteId { get; set; }
        public int GrupoId { get; set; }
        public int NewMateriaId { get; set; }
        public List<SelectListItem> MateriaOptions { get; set; }

        [TempData]
        public string Message { get; set; }

        private readonly IConfiguration _configuration;
        public EstudianteMateriaModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task OnGetAsync(int grupoId, int estudianteId)
        {
            EstudianteId = estudianteId;
            GrupoId = grupoId;

            await FillMateriaOptions(grupoId, estudianteId);
            var grupoServiceApi = GetIGrupoServiceApi();
            var grupoResponse = await grupoServiceApi.GetAsync(grupoId);

            if (grupoResponse.IsSuccessStatusCode)
            {
                NombreGrupo = grupoResponse.Content.Nombre;
            }

            var estudianteServiceApi = GetIEstudianteServiceApi();
            var estudianteResponse = await estudianteServiceApi.GetAsync(estudianteId);

            if (estudianteResponse.IsSuccessStatusCode)
            {
                NombreEstudiante = estudianteResponse.Content.Nombre + " " + estudianteResponse.Content.ApellidoPaterno + " " + estudianteResponse.Content.ApellidoMaterno;
            }

            var estudianteMateriaServiceApi = GetIEstudianteMateriaServiceApi();
            var estudianteMateriaResponse = await estudianteMateriaServiceApi.GetAllByEstudianteGrupoAsync(estudianteId, grupoId);

            if (estudianteMateriaResponse.IsSuccessStatusCode)
            {
                EstudianteMateriaModelView = estudianteMateriaResponse.Content.Data.Select(
                    x => new ModelViews.Estudiante.EstudianteMateriaModelView
                    {
                        EstudianteId = x.EstudianteId,
                        GrupoId = x.GrupoId,
                        MateriaId = x.MateriaId,
                        MateriaNombre = x.MateriaResponse.Nombre
                    });
            }
        }

        public async Task<JsonResult> OnGetCreateEstudianteMateriaAsync(int estudianteId, int grupoId, int materiaId)
        {
            var estudianteMateriaServiceApi = GetIEstudianteMateriaServiceApi();
            var respuestasAsaServiceResponse = await estudianteMateriaServiceApi.CreateAsync(new CIAC_TAS_Service.Contracts.V1.Requests.CreateEstudianteMateriaRequest
            {
                EstudianteId = estudianteId,
                GrupoId = grupoId,
                MateriaId = materiaId
            });

            if (!respuestasAsaServiceResponse.IsSuccessStatusCode)
            {
                return new JsonResult("Error al intentar asignar nueva Materia");
            }

            return new JsonResult("Creacion correcta");
        }

        public async Task<IActionResult> OnGetRemoveMateriaFromEstudianteAsync(int estudianteId, int grupoId, int materiaId)
        {
            var estudianteMateriaServiceApi = GetIEstudianteMateriaServiceApi();
            var estudianteMateriaResponse = await estudianteMateriaServiceApi.DeleteAsync(estudianteId, grupoId, materiaId);

            if (!estudianteMateriaResponse.IsSuccessStatusCode)
            {
                Message = "Ocurrio un error inesperado";

                return RedirectToPage("/Estudiante/EstudianteMateria", new { grupoId = grupoId, estudianteId = estudianteId });
            }

            return RedirectToPage("/Estudiante/EstudianteMateria", new { grupoId = grupoId, estudianteId = estudianteId });
        }

        public async Task<IActionResult> OnGetAsignarTodasEstudianteMateriaAsync(int estudianteId, int grupoId)
        {
            var estudianteMateriaServiceApi = GetIEstudianteMateriaServiceApi();
            var respuestasAsaServiceResponse = await estudianteMateriaServiceApi.CreateAsignAllMateriasAsync(estudianteId, grupoId);

            return RedirectToPage("/Estudiante/EstudianteMateria", new { grupoId = grupoId, estudianteId = estudianteId });
        }

        private async Task FillMateriaOptions(int grupoId, int estudianteId)
        {
            var materiaServiceApi = GetIMateriaServiceApi();
            var materiaResponse = await materiaServiceApi.GetAllNotAssignedMateriasAsync(estudianteId, grupoId);

            if (materiaResponse.IsSuccessStatusCode)
            {
                MateriaOptions = materiaResponse.Content.Data.Select(x => new SelectListItem { Text = x.Nombre, Value = x.Id.ToString() }).ToList();
            }
        }

        private IEstudianteMateriaServiceApi GetIEstudianteMateriaServiceApi()
        {
            return RestService.For<IEstudianteMateriaServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
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

        private IMateriaServiceApi GetIMateriaServiceApi()
        {
            return RestService.For<IMateriaServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
        }

        private IEstudianteServiceApi GetIEstudianteServiceApi()
        {
            return RestService.For<IEstudianteServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
        }
    }
}
