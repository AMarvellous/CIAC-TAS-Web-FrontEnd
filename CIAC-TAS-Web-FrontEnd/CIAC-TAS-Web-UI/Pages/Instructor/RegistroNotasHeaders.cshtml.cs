using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.Helper;
using CIAC_TAS_Web_UI.ModelViews.Estudiante;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Refit;

namespace CIAC_TAS_Web_UI.Pages.Instructor
{
    public class RegistroNotasHeadersModel : PageModel
    {
        [BindProperty]
        public List<RegistroNotaHeadersModelView> RegistroNotaHeadersModelView { get; set; } = new List<RegistroNotaHeadersModelView>();
        public string GrupoNombre { get; set; }
        public string MateriaNombre { get; set; }

        [TempData]
        public string Message { get; set; }

        private readonly IConfiguration _configuration;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment;

        public RegistroNotasHeadersModel(IConfiguration configuration, Microsoft.AspNetCore.Hosting.IHostingEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        public async Task<IActionResult> OnGetAsync(int grupoId, int materiaId)
        {
            var registroNotaHeaderServiceApi = GetIRegistroNotaHeaderServiceApi();
            var registroNotaHeaderResponse = await registroNotaHeaderServiceApi.GetAllHeadersByGrupoAndMateriaIdAsync(grupoId, materiaId);

            if (!registroNotaHeaderResponse.IsSuccessStatusCode)
            {
                Message = "Ocurrio un error inesperado";

                return RedirectToPage("/Instructor/RegistroNotasList");
            }

            var registroNotaHeaders = registroNotaHeaderResponse.Content.Data;
            RegistroNotaHeadersModelView = registroNotaHeaders.Select(x => new RegistroNotaHeadersModelView
            {
                Id = x.Id,
                GrupoId = x.GrupoId,
                MateriaId = x.MateriaId,
                ProgramaNombre = x.Programa.Nombre,
                GrupoNombre = x.Grupo.Nombre,
                MateriaNombre = x.Materia.Nombre,
                ModuloNombre = x.Modulo.Nombre,
                InstructorNombre = x.Instructor.Nombres
            }).ToList();

            var grupoServiceApi = GetIGrupoServiceApi();
            var grupoResponse = await grupoServiceApi.GetAsync(grupoId);
            if (grupoResponse.IsSuccessStatusCode)
            {
                GrupoNombre = grupoResponse.Content.Nombre;
            }

            var materiaServiceApi = GetIMateriaServiceApi();
            var materiaResponse = await materiaServiceApi.GetAsync(materiaId);
            if (materiaResponse.IsSuccessStatusCode)
            {
                MateriaNombre = materiaResponse.Content.Nombre;
            }

            return Page();
        }

        private IRegistroNotaHeaderServiceApi GetIRegistroNotaHeaderServiceApi()
        {
            return RestService.For<IRegistroNotaHeaderServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
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
    }
}
