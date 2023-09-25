using CIAC_TAS_Service.Contracts.V1.Responses;
using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.Helper;
using CIAC_TAS_Web_UI.ModelViews.General;
using CIAC_TAS_Web_UI.ModelViews.Instructores;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Refit;
using static CIAC_TAS_Service.Contracts.V1.ApiRoute;

namespace CIAC_TAS_Web_UI.Pages.Instructor
{
    public class InstructorMateriasModel : PageModel
    {
        [TempData]
        public string Message { get; set; }
        public int InstructorId { get; set; }
        public int NewGrupoId { get; set; }
        public int NewMateriaId { get; set; }
        public List<SelectListItem> GrupoOptions { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> MateriaOptions { get; set; } = new List<SelectListItem>();
        public List<InstructorMateriasModelView> InstructorMateriasModelView { get; set; } = new List<InstructorMateriasModelView>();
        public string InstructorNombre { get; set; }
        private readonly IConfiguration _configuration;
        public InstructorMateriasModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> OnGetViewInstructorMateriasAsync(int instructorId)
        {
            InstructorId = instructorId;

            var instructorServiceApi = GetIInstructorServiceApi();
            var instructorResponse = await instructorServiceApi.GetAsync(instructorId);

            if (instructorResponse.IsSuccessStatusCode)
            {
                InstructorNombre = instructorResponse.Content.Nombres + " " + instructorResponse.Content.ApellidoPaterno + " " + instructorResponse.Content.ApellidoMaterno;
            }

            var instructorMateriaServiceApi = GetIInstructorMateriaServiceApi();
            var instructorMateriaServiceResponse = await instructorMateriaServiceApi.GetAllByInstructorIdAsync(instructorId);

            if (instructorMateriaServiceResponse.IsSuccessStatusCode)
            {
                InstructorMateriasModelView = instructorMateriaServiceResponse
                    .Content
                    .Data
                    .Select(x => new InstructorMateriasModelView
                    {
                        InstructorId = x.InstructorId,
                        MateriaId = x.MateriaId,
                        GrupoId = x.GrupoId,
                        //InstructorNombre = x.Instructor.Nombres + " " + x.Instructor.ApellidoPaterno,
                        MateriaNombre = x.Materia.Nombre,
                        GrupoNombre = x.Grupo.Nombre,
                    }).ToList();
            }

            var grupoServiceApi = GetIGrupoServiceApiServiceApi();
            var grupoResponse = await grupoServiceApi.GetAllAsync();

            if (grupoResponse.IsSuccessStatusCode)
            {
                GrupoOptions = grupoResponse.Content.Data.Select(x => new SelectListItem { Text = x.Nombre, Value = x.Id.ToString() }).ToList();
            }

            var materiaServiceApi = GetIMateriaServiceApi();
            var materiaResponse = await materiaServiceApi.GetAllAsync();

            if (materiaResponse.IsSuccessStatusCode)
            {
                MateriaOptions = materiaResponse.Content.Data.Select(x => new SelectListItem { Text = x.Nombre, Value = x.Id.ToString() }).ToList();
            }

            return Page();
        }

        public async Task<IActionResult> OnGetNewMateriaGrupoAsync(int grupoId, int materiaId, int instructorId)
        {
            var instructorMateriaServiceApi = GetIInstructorMateriaServiceApi();
            await instructorMateriaServiceApi.CreateAsync(new CIAC_TAS_Service.Contracts.V1.Requests.CreateInstructorMateriaRequest
            {
                InstructorId = instructorId,
                MateriaId = materiaId,
                GrupoId = grupoId,
            });

            return RedirectToPage("/Instructor/InstructorMaterias", "ViewInstructorMaterias", new { instructorId = instructorId });
        }

        private IInstructorServiceApi GetIInstructorServiceApi()
        {
            return RestService.For<IInstructorServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
        }

        private IInstructorMateriaServiceApi GetIInstructorMateriaServiceApi()
        {
            return RestService.For<IInstructorMateriaServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
        }

        private IGrupoServiceApi GetIGrupoServiceApiServiceApi()
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
