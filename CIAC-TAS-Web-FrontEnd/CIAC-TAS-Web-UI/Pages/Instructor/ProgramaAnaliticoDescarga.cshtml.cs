using AspNetCore.Reporting;
using CIAC_TAS_Service.Contracts.V1.Responses;
using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.Helper;
using CIAC_TAS_Web_UI.ModelViews.Instructores;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Refit;
using System.Data;
using System.Text;
using static CIAC_TAS_Service.Contracts.V1.ApiRoute;
using static System.Collections.Specialized.BitVector32;

namespace CIAC_TAS_Web_UI.Pages.Instructor
{
    public class ProgramaAnaliticoDescargaModel : PageModel
    {
        [TempData]
        public string Message { get; set; }
        public int InstructorId { get; set; }
        public IEnumerable<ProgramaAnaliticoPdfModelView> ProgramaAnaliticoPdfModelView { get; set; } = new List<ProgramaAnaliticoPdfModelView>();
        private readonly IConfiguration _configuration;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment;
        public ProgramaAnaliticoDescargaModel(IConfiguration configuration, Microsoft.AspNetCore.Hosting.IHostingEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        public async Task OnGetAsync()
        {
            var userId = HttpContext.Session.GetString(Session.SessionUserId);
            var instructorServiceApi = GetIInstructorServiceApi();
            var instructorResponse = await instructorServiceApi.GetByUserIdAsync(userId);

            var instructorId = 0;
            if (instructorResponse.IsSuccessStatusCode)
            {
                instructorId = instructorResponse.Content.Id;
            }

            InstructorId = instructorId;
            var instructorProgramaAnaliticoServiceApi = GetIInstructorProgramaAnaliticoServiceApi();
            var instructorProgramaAnaliticoResponse = await instructorProgramaAnaliticoServiceApi.GetAllByInstructorIdAsync(instructorId);

            if (instructorProgramaAnaliticoResponse.IsSuccessStatusCode)
            {
                ProgramaAnaliticoPdfModelView = instructorProgramaAnaliticoResponse.Content.Data.Select(x => new ProgramaAnaliticoPdfModelView
                {
                    Id = x.ProgramaAnaliticoPdfId, //Programa Analitico Pdf Id
                    //RutaPdf = x.ProgramaAnaliticoPdf.RutaPdf,
                    //MateriaId = x.ProgramaAnaliticoPdf.MateriaId,
                    MateriaNombre = x.ProgramaAnaliticoPdf.Materia.Nombre,
                    Gestion = x.ProgramaAnaliticoPdf.Gestion,
                });
            }
        }

        public async Task<IActionResult> OnGetDownloadProgramaAnaliticoPdfAsync(int instructorId, int programaAnaliticoId)
        {
            var instructorProgramaAnaliticoServiceApi = GetIInstructorProgramaAnaliticoServiceApi();
            var instructorProgramaAnaliticoResponse = await instructorProgramaAnaliticoServiceApi.GetAsync(instructorId, programaAnaliticoId);

            var filePath = string.Empty;
            var nombreDescarga = string.Empty;
            if (instructorProgramaAnaliticoResponse.IsSuccessStatusCode)
            {
                nombreDescarga = instructorProgramaAnaliticoResponse.Content.ProgramaAnaliticoPdf.Materia.Nombre + "_" +
                    instructorProgramaAnaliticoResponse.Content.ProgramaAnaliticoPdf.Gestion + "_" + DateTime.Now.Ticks + ".pdf";
                var nombreArchivo = instructorProgramaAnaliticoResponse.Content.ProgramaAnaliticoPdf.RutaPdf;
                filePath = Path.Combine(_environment.WebRootPath, "dist/uploads/ProgramaAnalitico", nombreArchivo);
            }

            return File(System.IO.File.ReadAllBytes(filePath), "application/pdf", nombreDescarga);
        }

        private IInstructorProgramaAnaliticoServiceApi GetIInstructorProgramaAnaliticoServiceApi()
        {
            return RestService.For<IInstructorProgramaAnaliticoServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
        }

        private IInstructorServiceApi GetIInstructorServiceApi()
        {
            return RestService.For<IInstructorServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
        }
    }
}
