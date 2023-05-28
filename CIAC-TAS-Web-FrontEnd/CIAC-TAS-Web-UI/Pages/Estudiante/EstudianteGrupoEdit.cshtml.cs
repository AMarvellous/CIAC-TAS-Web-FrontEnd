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
    public class EstudianteGrupoEditModel : PageModel
    {
        [BindProperty]
        public List<EstudianteGrupoModelView> EstudianteGrupoModelView { get; set; } = new List<EstudianteGrupoModelView>();
        public string EditGrupoNombre { get; set; }
        public int GrupoId { get; set; }
        public List<SelectListItem> GrupoOptions { get; set; }

        [TempData]
        public string Message { get; set; }

        private readonly IConfiguration _configuration;
        public EstudianteGrupoEditModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> OnGetEditEstudianteGrupoAsync(int grupoId)
        {
            if (grupoId == 0)
            {
                Message = "Ocurrio un error inesperado";
                return RedirectToPage("/Estudiante/EstudiantesGrupos");
            }

            var estudiantegrupoServiceApi = GetIEstudianteGrupoServiceApi();
            var estudianteGrupoResponse = await estudiantegrupoServiceApi.GetAllByGrupoIdAsync(grupoId);

            if (!estudianteGrupoResponse.IsSuccessStatusCode)
            {
                Message = "Ocurrio un error inesperado";

                return RedirectToPage("/Estudiante/EstudiantesGrupos");
            }

            var estudianteGrupos = estudianteGrupoResponse.Content.Data;

            foreach (var estudianteGrupo in estudianteGrupos)
            {
                EstudianteGrupoModelView.Add(new ModelViews.Estudiante.EstudianteGrupoModelView
                {
                    EstudianteId = estudianteGrupo.EstudianteId,
                    GrupoId = estudianteGrupo.GrupoId,
                    EstudianteNombre = estudianteGrupo.EstudianteResponse.Nombre + " " + estudianteGrupo.EstudianteResponse.ApellidoPaterno,
                    GrupoNombre = estudianteGrupo.GrupoResponse.Nombre
                });                
            }

            var grupoServiceApi = GetIGrupoServiceApi();
            var grupoResponse = await grupoServiceApi.GetAsync(grupoId);
            
            if (grupoResponse.IsSuccessStatusCode)
            {
                EditGrupoNombre = grupoResponse.Content.Nombre;
            }

            GrupoId = grupoId;

            return Page();
        }        

        public async Task<IActionResult> OnGetRemoveEstudianteFromGrupoAsync(int estudianteId, int grupoId)
        {
            var estudiantegrupoServiceApi = GetIEstudianteGrupoServiceApi();
            var grupoResponse = await estudiantegrupoServiceApi.DeleteAsync(estudianteId, grupoId);

            if (!grupoResponse.IsSuccessStatusCode)
            {
                Message = "Ocurrio un error inesperado";

                return RedirectToPage("/Estudiante/EstudianteGrupoEdit", "EditEstudianteGrupo", new { grupoId = grupoId });
            }

            return RedirectToPage("/Estudiante/EstudianteGrupoEdit", "EditEstudianteGrupo", new { grupoId = grupoId });
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
