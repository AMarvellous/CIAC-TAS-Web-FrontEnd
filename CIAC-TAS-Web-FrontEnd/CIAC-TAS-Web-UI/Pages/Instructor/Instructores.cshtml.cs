using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.Helper;
using CIAC_TAS_Web_UI.ModelViews.Estudiante;
using CIAC_TAS_Web_UI.ModelViews.General;
using CIAC_TAS_Web_UI.ModelViews.Instructores;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Refit;

namespace CIAC_TAS_Web_UI.Pages.Instructor
{
    public class InstructoresModel : PageModel
    {
		[BindProperty]
		public IEnumerable<InstructorModelView> InstructorModelView { get; set; } = new List<InstructorModelView>();

		[TempData]
		public string Message { get; set; }

		private readonly IConfiguration _configuration;
		public InstructoresModel(IConfiguration configuration)
		{
			_configuration = configuration;
		}

        public async Task<IActionResult> OnGetAsync()
        {
            var instructorServiceApi = GetInstructorServiceApi();
            var instructorResponse = await instructorServiceApi.GetAllAsync();

            if (!instructorResponse.IsSuccessStatusCode)
            {
                Message = "Ocurrio un error inesperado";

                return Page();
            }

            var instructors = instructorResponse.Content.Data;
            InstructorModelView = instructors.Select(x => new InstructorModelView
            {
                Id = x.Id,
                Nombres = x.Nombres + " " + x.ApellidoPaterno + " " + x.ApellidoMaterno,
                NumeroLicencia = x.NumeroLicencia
            });

            return Page();
        }

        public IInstructorServiceApi GetInstructorServiceApi()
        {
            return RestService.For<IInstructorServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
        }
    }
}
