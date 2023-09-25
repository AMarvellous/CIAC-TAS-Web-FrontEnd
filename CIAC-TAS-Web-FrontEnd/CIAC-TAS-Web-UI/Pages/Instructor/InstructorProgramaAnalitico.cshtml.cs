using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.Helper;
using CIAC_TAS_Web_UI.ModelViews.Instructores;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Refit;

namespace CIAC_TAS_Web_UI.Pages.Instructor
{
    public class InstructorProgramaAnaliticoModel : PageModel
    {
        [TempData]
        public string Message { get; set; }
        public IEnumerable<InstructorProgramaAnaliticoModelView> InstructorProgramaAnaliticoModelView = new List<InstructorProgramaAnaliticoModelView>();
        public List<SelectListItem> ProgramaAnaliticoOptions { get; set; }
        public string InstructorNombre { get; set; }
        public int InstructorId { get; set; }
        public int NewProgramaAnaliticoId { get; set; }
        private readonly IConfiguration _configuration;
        public InstructorProgramaAnaliticoModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task OnGetViewInstructorProgramaAnaliticoAsync(int instructorId, string instructorNombre)
        {
            var instructorProgramaAnaliticoServiceApi = GetIInstructorProgramaAnaliticoServiceApi();
            var instructorProgramaAnaliticoResponse = await instructorProgramaAnaliticoServiceApi.GetAllByInstructorIdAsync(instructorId);
            InstructorNombre = instructorNombre;
            InstructorId = instructorId;

            if (instructorProgramaAnaliticoResponse.IsSuccessStatusCode)
            {
                InstructorProgramaAnaliticoModelView = instructorProgramaAnaliticoResponse.Content.Data.Select(x => new ModelViews.Instructores.InstructorProgramaAnaliticoModelView
                {
                    InstructorId = x.InstructorId,
                    InstructorNombre = instructorNombre,
                    ProgramaAnaliticoPdfId = x.ProgramaAnaliticoPdfId,
                    ProgramaAnaliticoPdfRuta = x.ProgramaAnaliticoPdf.RutaPdf,
                    ProgramaAnaliticoPdfMateriaId = x.ProgramaAnaliticoPdf.MateriaId,
                    ProgramaAnaliticoPdfMateriaNombre = x.ProgramaAnaliticoPdf.Materia.Nombre,
                    ProgramaAnaliticoPdfGestion = x.ProgramaAnaliticoPdf.Gestion,
                });
            }

            var programaAnaliticoPdfServiceApi = GetIProgramaAnaliticoPdfServiceApi();
            var programaAnaliticoPdfResponse = await programaAnaliticoPdfServiceApi.GetAllNotAssignedInstructorAsync(instructorId);

            if (programaAnaliticoPdfResponse.IsSuccessStatusCode)
            {
                ProgramaAnaliticoOptions = programaAnaliticoPdfResponse.Content.Data
                    .Select(x => new SelectListItem { Text = x.Materia.Nombre + " - " + x.Gestion, Value = x.Id.ToString() }).ToList();
            }
        }

        public async Task<JsonResult> OnGetCreateInstructorProgramaAnaliticoAsync(int instructorId, int programaAnaliticoId)
        {
            var instructorProgramaAnaliticoServiceApi = GetIInstructorProgramaAnaliticoServiceApi();
            var respuestasAsaServiceResponse = await instructorProgramaAnaliticoServiceApi.CreateAsync(new CIAC_TAS_Service.Contracts.V1.Requests.CreateInstructorProgramaAnaliticoRequest
            {
                InstructorId = instructorId,
                ProgramaAnaliticoPdfId = programaAnaliticoId,
            });

            if (!respuestasAsaServiceResponse.IsSuccessStatusCode)
            {
                return new JsonResult("Error al intentar asignar un nuevo Programa Analitico");
            }

            return new JsonResult("Creacion correcta");
        }

        public async Task<IActionResult> OnGetDeleteInstructorProgramaAnaliticoPdfAsync(int instructorId, int programaAnaliticoId)
        {
            if (programaAnaliticoId == 0 || instructorId == 0)
            {
                Message = "Ocurrio un error al borrar el registro";

                return Page();
            }

            var instructorProgramaAnaliticoServiceApi = GetIInstructorProgramaAnaliticoServiceApi();
            var instructorProgramaAnaliticoResponse = await instructorProgramaAnaliticoServiceApi.DeleteAsync(instructorId, programaAnaliticoId);

            return RedirectToPage("/Instructor/InstructorProgramaAnalitico", "ViewInstructorProgramaAnalitico", new { instructorId = instructorId, programaAnaliticoId = programaAnaliticoId });
        }

        private IInstructorProgramaAnaliticoServiceApi GetIInstructorProgramaAnaliticoServiceApi()
        {
            return RestService.For<IInstructorProgramaAnaliticoServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
        }

        private IProgramaAnaliticoPdfServiceApi GetIProgramaAnaliticoPdfServiceApi()
        {
            return RestService.For<IProgramaAnaliticoPdfServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
        }
    }
}
