using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Refit;

namespace CIAC_TAS_Web_UI.Pages.Instructor
{
    public class InstructorProgramaAnaliticoPreviewModel : PageModel
    {
        public List<SelectListItem> InstructoresOptions { get; set; }

        [TempData]
        public string Message { get; set; }

        private readonly IConfiguration _configuration;
        public InstructorProgramaAnaliticoPreviewModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task OnGetAsync()
        {
            var instructorServiceApi = GetIInstructorServiceApi();
            var instructorServiceResponse = await instructorServiceApi.GetAllAsync();

            if (instructorServiceResponse.IsSuccessStatusCode)
            {
                InstructoresOptions = instructorServiceResponse.Content.Data.Select(x => new SelectListItem { Text = x.Nombres + " " + x.ApellidoPaterno, Value = x.Id.ToString() }).ToList();
            }
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
