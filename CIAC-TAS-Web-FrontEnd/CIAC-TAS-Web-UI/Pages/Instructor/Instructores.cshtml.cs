using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.ModelViews.Estudiante;
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
	}
}
