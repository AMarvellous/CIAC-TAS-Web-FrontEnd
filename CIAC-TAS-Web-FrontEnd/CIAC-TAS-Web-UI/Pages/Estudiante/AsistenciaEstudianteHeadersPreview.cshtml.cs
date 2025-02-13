using CIAC_TAS_Service.Contracts.V1.Responses;
using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.Helper;
using CIAC_TAS_Web_UI.ModelViews.General;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Refit;
using static CIAC_TAS_Service.Contracts.V1.ApiRoute;

namespace CIAC_TAS_Web_UI.Pages.Estudiante
{
    public class AsistenciaEstudianteHeadersPreviewModel : PageModel
    {
        public List<SelectListItem> GrupoOptions { get; set; }
        public List<SelectListItem> MateriaOptions { get; set; }
        [BindProperty]
        public int GrupoId { get; set; }
        [BindProperty]
        public int MateriaId { get; set; }
        [BindProperty]
        public bool IsAdmin { get; set; } = false;
        [TempData]
        public string Message { get; set; }

        private readonly IConfiguration _configuration;
        private readonly InstructorSession _instructorSession;
        public AsistenciaEstudianteHeadersPreviewModel(IConfiguration configuration, InstructorSession instructorSession)
        {
            _configuration = configuration;
            _instructorSession = instructorSession;
        }

        public async Task OnGetAsync()
        {
            IsAdmin = false;
            var roles = HttpContext.Session.GetString(Session.SessionRoles);
            if (roles.Contains(RolesHelper.ADMIN_ROLE))
            {
                IsAdmin = true;
            }

            if (IsAdmin)
            {
                await FillGrupoOptions();
                await FillMateriaOptions();
            } else
            {
                var userId = HttpContext.Session.GetString(Session.SessionUserId);
                var sessionToken = HttpContext.Session.GetString(Session.SessionToken);
                var instructorId = await _instructorSession.GetInstructorIdByAsync(userId, sessionToken);
                await FillGrupoOptionsAssignedInstructorMateria(instructorId);
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (GrupoId == 0)
            {
                Message = "Seleccione un Grupo para continuar";

                if (IsAdmin)
                {
                    await FillGrupoOptions();
                    await FillMateriaOptions();
                } else
                {
                    var userId = HttpContext.Session.GetString(Session.SessionUserId);
                    var sessionToken = HttpContext.Session.GetString(Session.SessionToken);
                    var instructorId = await _instructorSession.GetInstructorIdByAsync(userId, sessionToken);
                    await FillGrupoOptionsAssignedInstructorMateria(instructorId);
                }     

                return Page();
            }

            if (MateriaId == 0)
            {
                Message = "Seleccione una Materia para continuar";

                if (IsAdmin)
                {
                    await FillGrupoOptions();
                    await FillMateriaOptions();
                } else 
                {
                    var userId = HttpContext.Session.GetString(Session.SessionUserId);
                    var sessionToken = HttpContext.Session.GetString(Session.SessionToken);
                    var instructorId = await _instructorSession.GetInstructorIdByAsync(userId, sessionToken);
                    await FillGrupoOptionsAssignedInstructorMateria(instructorId);
                    await FillMateriaOptionsAssignedInstructorMateria(instructorId, GrupoId);
                }
                
                return Page();
            }

            var cierreMateriaServiceApi = GetICierreMateriaServiceApi();
            var cierreMateriaResponse = await cierreMateriaServiceApi.GetByGrupoIdMateriaIdAsync(GrupoId, MateriaId);

            if (!cierreMateriaResponse.IsSuccessStatusCode)
            {
                Message = "Ocurrio un error inesperado";

                if (IsAdmin)
                {
                    await FillGrupoOptions();
                    await FillMateriaOptions();
                }
                else
                {
                    var userId = HttpContext.Session.GetString(Session.SessionUserId);
                    var sessionToken = HttpContext.Session.GetString(Session.SessionToken);
                    var instructorId = await _instructorSession.GetInstructorIdByAsync(userId, sessionToken);
                    await FillGrupoOptionsAssignedInstructorMateria(instructorId);
                    await FillMateriaOptionsAssignedInstructorMateria(instructorId, GrupoId);
                }

                return Page();
            }

            // If cierreMateria exists, not continue unless is Admin
            if (cierreMateriaResponse.Content != null && !IsAdmin)
            {
                Message = "La Materia para el Grupo seleccionado ya se encuentra cerrada";

                if (IsAdmin)
                {
                    await FillGrupoOptions();
                    await FillMateriaOptions();
                }
                else
                {
                    var userId = HttpContext.Session.GetString(Session.SessionUserId);
                    var sessionToken = HttpContext.Session.GetString(Session.SessionToken);
                    var instructorId = await _instructorSession.GetInstructorIdByAsync(userId, sessionToken);
                    await FillGrupoOptionsAssignedInstructorMateria(instructorId);
                    await FillMateriaOptionsAssignedInstructorMateria(instructorId, GrupoId);
                }

                return Page();
            }

            return RedirectToPage("/Estudiante/AsistenciaEstudianteHeaders", new { grupoId = GrupoId, materiaId = MateriaId });
        }

        private async Task FillGrupoOptions()
        {
            var grupoServiceApi = GetIGrupoServiceApi();
            var grupoResponse = await grupoServiceApi.GetAllAsync();

            if (grupoResponse.IsSuccessStatusCode)
            {
                GrupoOptions = grupoResponse.Content.Data.Select(x => new SelectListItem { Text = x.Nombre, Value = x.Id.ToString() }).ToList();
            }
        }

        private async Task FillMateriaOptions()
        {
            var materiaServiceApi = GetIMateriaServiceApi();
            var materiaResponse = await materiaServiceApi.GetAllAsync();

            if (materiaResponse.IsSuccessStatusCode)
            {
                MateriaOptions = materiaResponse.Content.Data.Select(x => new SelectListItem { Text = x.Nombre, Value = x.Id.ToString() }).ToList();
            }
        }

        private async Task FillGrupoOptionsAssignedInstructorMateria(int instructorId)
        {
            var grupoServiceApi = GetIGrupoServiceApi();
            var grupoResponse = await grupoServiceApi.GetAllGruposAssignedByInstructorAsync(instructorId);

            if (grupoResponse.IsSuccessStatusCode)
            {
                GrupoOptions = grupoResponse.Content.Data.Select(x => new SelectListItem { Text = x.Nombre, Value = x.Id.ToString() }).ToList();
            }
        }

        private async Task<Dictionary<string, string>> FillMateriaOptionsAssignedInstructorMateria(int instructorId, int grupoId)
        {
            Dictionary<string, string> optionsDictionary = new Dictionary<string, string>();
            var materiaServiceApi = GetIMateriaServiceApi();
            var materiaResponse = await materiaServiceApi.GetAllMateriasAssignedByInstructorGrupoAsync(instructorId, grupoId);

            if (materiaResponse.IsSuccessStatusCode)
            {
                MateriaOptions = materiaResponse.Content.Data.Select(x => new SelectListItem { Text = x.Nombre, Value = x.Id.ToString() }).ToList();
                materiaResponse.Content.Data.ToList().ForEach(x =>
                    optionsDictionary.Add(x.Id.ToString(), x.Nombre)
                );
            }

            return optionsDictionary;
        }

        public async Task<JsonResult> OnGetGetMateriaOptionsByGrupoIdAsync(int grupoId)
        {
            var userId = HttpContext.Session.GetString(Session.SessionUserId);
            var sessionToken = HttpContext.Session.GetString(Session.SessionToken);
            var instructorId = await _instructorSession.GetInstructorIdByAsync(userId, sessionToken);
            Dictionary<string, string> optionsDictionary = await FillMateriaOptionsAssignedInstructorMateria(instructorId, grupoId);

            return new JsonResult(optionsDictionary);
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

        private ICierreMateriaServiceApi GetICierreMateriaServiceApi()
        {
            return RestService.For<ICierreMateriaServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
        }
    }
}
