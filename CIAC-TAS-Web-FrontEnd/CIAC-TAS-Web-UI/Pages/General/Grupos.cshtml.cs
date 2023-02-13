using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.Helper;
using CIAC_TAS_Web_UI.ModelViews.Estudiante;
using CIAC_TAS_Web_UI.ModelViews.General;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Refit;

namespace CIAC_TAS_Web_UI.Pages.General
{
    public class GruposModel : PageModel
    {
		[BindProperty]
		public IEnumerable<GrupoModelView> GrupoModelView { get; set; } = new List<GrupoModelView>();

		[TempData]
		public string Message { get; set; }

		private readonly IConfiguration _configuration;
		public GruposModel(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public async Task<IActionResult> OnGetAsync()
		{
			var grupoServiceApi = GetGrupoServiceApi();
			var grupoResponse = await grupoServiceApi.GetAllAsync();

			if (!grupoResponse.IsSuccessStatusCode)
			{
				Message = "Ocurrio un error inesperado";

				return Page();
			}

			var grupos = grupoResponse.Content.Data;
			GrupoModelView = grupos.Select(x => new GrupoModelView
			{
				Id = x.Id,
				Nombre = x.Nombre
			});

			return Page();
		}

		public async Task<IActionResult> OnGetRemoveGrupoAsync(int id)
		{
			var grupoServiceApi = GetGrupoServiceApi();
			var grupoResponse = await grupoServiceApi.DeleteAsync(id);

			if (!grupoResponse.IsSuccessStatusCode)
			{
				Message = "Ocurrio un error inesperado";

				return RedirectToPage("/General/Grupos");
			}

			return RedirectToPage("/General/Grupos");
		}

		public IGrupoServiceApi GetGrupoServiceApi()
		{
			return RestService.For<IGrupoServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
			{
				AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
			});
		}
	}
}
