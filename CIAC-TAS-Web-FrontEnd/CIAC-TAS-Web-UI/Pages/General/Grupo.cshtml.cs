using CIAC_TAS_Service.Contracts.V1.Responses;
using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.Helper;
using CIAC_TAS_Web_UI.ModelViews.Estudiante;
using CIAC_TAS_Web_UI.ModelViews.General;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Refit;

namespace CIAC_TAS_Web_UI.Pages.General
{
    public class GrupoModel : PageModel
    {
		[BindProperty]
		public GrupoModelView GrupoModelView { get; set; }

		[TempData]
		public string Message { get; set; }

		private readonly IConfiguration _configuration;
		public GrupoModel(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public async Task<IActionResult> OnGetNewGrupoAsync()
		{
			return Page();
		}

		public async Task<IActionResult> OnPostNewGrupoAsync()
		{
			if (!ModelState.IsValid)
			{
				Message = "Por favor complete el formulario correctamente";

				return Page();
			}

			var grupoServiceApi = GetIGrupoServiceApi();
			var createServiceResponse = await grupoServiceApi.CreateAsync(new CIAC_TAS_Service.Contracts.V1.Requests.CreateGrupoRequest
			{
				Nombre = GrupoModelView.Nombre
			});

			if (!createServiceResponse.IsSuccessStatusCode)
			{
				var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(createServiceResponse.Error.Content);

				Message = String.Join(" ", errorResponse.Errors.Select(x => x.Message));

				return Page();
			}

			return RedirectToPage("/General/Grupos");
		}

		public async Task<IActionResult> OnGetEditGrupoAsync(int id)
		{
			var grupoServiceApi = GetIGrupoServiceApi();
			var grupoServiceResponse = await grupoServiceApi.GetAsync(id);

			if (!grupoServiceResponse.IsSuccessStatusCode)
			{
				Message = "Ocurrio un error inesperado";

				return RedirectToPage("/General/Grupos");
			}

			var grupoResponse = grupoServiceResponse.Content;
			GrupoModelView = new GrupoModelView
			{
				Id = grupoResponse.Id,
				Nombre = grupoResponse.Nombre,
			};

			return Page();
		}

		public async Task<IActionResult> OnPostEditGrupoAsync(int id)
		{
			if (!ModelState.IsValid)
			{
				Message = "Por favor complete el formulario correctamente";

				return Page();
			}

			var grupoServiceApi = GetIGrupoServiceApi();

			var grupoServiceResponse = await grupoServiceApi.UpdateAsync(id, new CIAC_TAS_Service.Contracts.V1.Requests.UpdateGrupoRequest
			{
				Nombre = GrupoModelView.Nombre
			});

			if (!grupoServiceResponse.IsSuccessStatusCode)
			{
				var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(grupoServiceResponse.Error.Content);

				Message = String.Join(" ", errorResponse.Errors.Select(x => x.Message));

				return Page();
			}

			return RedirectToPage("/General/Grupos");
		}
		
		private IGrupoServiceApi GetIGrupoServiceApi()
		{
			return RestService.For<IGrupoServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
			{
				AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
			});
		}
	}
}
