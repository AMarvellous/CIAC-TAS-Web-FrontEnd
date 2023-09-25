using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.Helper;
using CIAC_TAS_Web_UI.ModelViews.Instructores;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Refit;

namespace CIAC_TAS_Web_UI.Pages.Instructor
{
    public class ProgramasAnaliticosPdfModel : PageModel
    {
        [TempData]
        public string Message { get; set; }
        public IEnumerable<ProgramaAnaliticoPdfModelView> ProgramaAnaliticoPdfModelView { get; set; } = new List<ProgramaAnaliticoPdfModelView>();
        private readonly IConfiguration _configuration;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment;
        public ProgramasAnaliticosPdfModel(IConfiguration configuration, Microsoft.AspNetCore.Hosting.IHostingEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        public async Task OnGetAsync()
        {
            var programaAnaliticoPdfServiceApi = GetIProgramaAnaliticoPdfServiceApi();
            var programaAnaliticoPdfResponse = await programaAnaliticoPdfServiceApi.GetAllAsync();

            if (programaAnaliticoPdfResponse.IsSuccessStatusCode)
            {
                ProgramaAnaliticoPdfModelView = programaAnaliticoPdfResponse.Content.Data.Select(x => new ProgramaAnaliticoPdfModelView
                {
                    Id = x.Id,
                    RutaPdf = x.RutaPdf,
                    MateriaId = x.MateriaId,
                    MateriaNombre = x.Materia.Nombre,
                    Gestion = x.Gestion,
                });
            }
        }

        public async Task<IActionResult> OnGetDeleteProgramaAnaliticoPdfAsync(int programaAnaliticoId, string rutaPdf)
        {
            if (programaAnaliticoId == 0)
            {
                Message = "Ocurrio un error al borrar el registro";

                return Page();
            }

            var programaAnaliticoPdfServiceApi = GetIProgramaAnaliticoPdfServiceApi();
            var programaAnaliticoPdfResponse = await programaAnaliticoPdfServiceApi.DeleteAsync(programaAnaliticoId);

            if (programaAnaliticoPdfResponse.IsSuccessStatusCode)
            {
                //Borramos el pdf                
                string programaAnaliticoContainerPath = Path.Combine(_environment.WebRootPath, "dist/uploads/ProgramaAnalitico");
                var fullPath = Path.Combine(programaAnaliticoContainerPath, rutaPdf);

                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }

            return RedirectToPage("/Instructor/ProgramasAnaliticosPdf");
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
