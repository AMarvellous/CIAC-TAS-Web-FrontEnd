using CIAC_TAS_Service.Contracts.V1.Responses;
using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.Helper;
using CIAC_TAS_Web_UI.ModelViews.Instructores;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Refit;
using System;

namespace CIAC_TAS_Web_UI.Pages.Instructor
{
    public class ProgramaAnaliticoPdfModel : PageModel
    {
        [TempData]
        public string Message { get; set; }
        [BindProperty]
        public IFormFile? UploadFile { get; set; }
        [BindProperty]
        public ProgramaAnaliticoPdfModelView ProgramaAnaliticoPdfModelView { get; set; }
        public List<SelectListItem> MateriaOptions { get; set; }
        private readonly IConfiguration _configuration;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment;
        public ProgramaAnaliticoPdfModel(IConfiguration configuration, Microsoft.AspNetCore.Hosting.IHostingEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        public async Task OnGetAsync()
        {
            await FillMateriaOptions();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Message = "Por favor complete el formulario correctamente";

                await FillMateriaOptions();
                return Page();
            }

            var fullPath = string.Empty;
            if (UploadFile != null && !string.IsNullOrEmpty(UploadFile.FileName))
            {
                var fileName = DateTime.Now.Ticks + "_" + UploadFile.FileName;
                string programaAnaliticoContainerPath = Path.Combine(_environment.WebRootPath, "dist/uploads/ProgramaAnalitico");
                fullPath = Path.Combine(programaAnaliticoContainerPath, fileName);
                ProgramaAnaliticoPdfModelView.RutaPdf = fileName;

                if (!Directory.Exists(programaAnaliticoContainerPath))
                {
                    Directory.CreateDirectory(programaAnaliticoContainerPath);
                } 
            } else {
                Message = "Por favor complete el formulario correctamente";

                await FillMateriaOptions();
                return Page();
            }


            var programaAnaliticoPdfServiceApi = GetIProgramaAnaliticoPdfServiceApi();
            var programaAnaliticoPdfResponse = await programaAnaliticoPdfServiceApi.CreateAsync(new CIAC_TAS_Service.Contracts.V1.Requests.CreateProgramaAnaliticoPdfRequest
            {
                RutaPdf = ProgramaAnaliticoPdfModelView.RutaPdf,
                MateriaId = ProgramaAnaliticoPdfModelView.MateriaId,
                Gestion = ProgramaAnaliticoPdfModelView.Gestion
            });

            if (!programaAnaliticoPdfResponse.IsSuccessStatusCode)
            {
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(programaAnaliticoPdfResponse.Error.Content);

                Message = String.Join(" ", errorResponse.Errors.Select(x => x.Message));

                await FillMateriaOptions();

                return Page();
            } else
            {
                using (var fileStream = new FileStream(fullPath, FileMode.CreateNew))
                {
                    await UploadFile.CopyToAsync(fileStream);
                }
            }

            return RedirectToPage("/Instructor/ProgramasAnaliticosPdf");
        }

        public async Task OnGetEditProgramaAnaliticoPdf(int programaAnaliticoId)
        {
            await FillMateriaOptions();

            var programaAnaliticoPdfServiceApi = GetIProgramaAnaliticoPdfServiceApi();
            var programaAnaliticoPdfResponse = await programaAnaliticoPdfServiceApi.GetAsync(programaAnaliticoId);

            ProgramaAnaliticoPdfModelView = new ProgramaAnaliticoPdfModelView();
            
            if (programaAnaliticoPdfResponse.IsSuccessStatusCode)
            {
                ProgramaAnaliticoPdfModelView.Id = programaAnaliticoId;
                ProgramaAnaliticoPdfModelView.Gestion = programaAnaliticoPdfResponse.Content.Gestion;
                ProgramaAnaliticoPdfModelView.MateriaId = programaAnaliticoPdfResponse.Content.MateriaId;
                ProgramaAnaliticoPdfModelView.RutaPdf = programaAnaliticoPdfResponse.Content.RutaPdf;
            }
        }

        public async Task<IActionResult> OnPostEditProgramaAnaliticoAsync(int programaAnaliticoId, string rutaPdf)
        {
            if (!ModelState.IsValid)
            {
                Message = "Por favor complete el formulario correctamente";

                await FillMateriaOptions();
                return Page();
            }

            ProgramaAnaliticoPdfModelView.RutaPdf = rutaPdf;
            var programaAnaliticoPdfServiceApi = GetIProgramaAnaliticoPdfServiceApi();
            var programaAnaliticoPdfResponse = await programaAnaliticoPdfServiceApi.UpdateAsync(programaAnaliticoId, new CIAC_TAS_Service.Contracts.V1.Requests.UpdateProgramaAnaliticoPdfRequest
            {
                RutaPdf = ProgramaAnaliticoPdfModelView.RutaPdf,
                MateriaId = ProgramaAnaliticoPdfModelView.MateriaId,
                Gestion = ProgramaAnaliticoPdfModelView.Gestion
            });

            if (!programaAnaliticoPdfResponse.IsSuccessStatusCode)
            {
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(programaAnaliticoPdfResponse.Error.Content);

                Message = String.Join(" ", errorResponse.Errors.Select(x => x.Message));

                await FillMateriaOptions();

                return Page();
            }

            return RedirectToPage("/Instructor/ProgramasAnaliticosPdf");
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

        private IMateriaServiceApi GetIMateriaServiceApi()
        {
            return RestService.For<IMateriaServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
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
