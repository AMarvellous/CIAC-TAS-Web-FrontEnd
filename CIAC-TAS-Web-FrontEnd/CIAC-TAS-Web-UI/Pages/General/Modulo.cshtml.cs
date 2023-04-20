using CIAC_TAS_Service.Contracts.V1.Responses;
using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.Helper;
using CIAC_TAS_Web_UI.ModelViews.General;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Refit;

namespace CIAC_TAS_Web_UI.Pages.General
{
    public class ModuloModel : PageModel
    {
		[BindProperty]
		public ModuloModelView ModuloModelView { get; set; }


		[TempData]
		public string Message { get; set; }

		private readonly IConfiguration _configuration;
		public ModuloModel(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public async Task OnGetNewModuloAsync()
		{

		}

		public async Task<IActionResult> OnPostNewModuloAsync()
		{
			if (!ModelState.IsValid)
			{
				Message = "Por favor complete el formulario correctamente";

				return Page();
			}

			var moduloApi = GetIModuloServiceApi();
			var moduloRequest = new CIAC_TAS_Service.Contracts.V1.Requests.CreateModuloRequest
			{
				Nombre = ModuloModelView.Nombre,
				ModuloCodigo = ModuloModelView.ModuloCodigo,
			};

			var moduloResponse = await moduloApi.CreateAsync(moduloRequest);
			if (!moduloResponse.IsSuccessStatusCode)
			{
				var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(moduloResponse.Error.Content);

				Message = String.Join(" ", errorResponse.Errors.Select(x => x.Message));

				return Page();
			}

			return RedirectToPage("/General/Modulos");
		}

		public async Task<IActionResult> OnGetEditModuloAsync(int id)
		{
			var moduloApi = GetIModuloServiceApi();
			var moduloResponse = await moduloApi.GetAsync(id);

			if (!moduloResponse.IsSuccessStatusCode)
			{
				Message = "Ocurrio un error inesperado";

				return RedirectToPage("/General/Modulos");
			}

			var modulo = moduloResponse.Content;
			ModuloModelView = new ModuloModelView
			{
				Id = modulo.Id,
				Nombre = modulo.Nombre,
				ModuloCodigo = modulo.ModuloCodigo,
			};

			return Page();
		}
		public async Task<IActionResult> OnPostEditModuloAsync(int id)
		{
			if (!ModelState.IsValid)
			{
				Message = "Por favor complete el formulario correctamente";

				return Page();
			}

			var moduloApi = GetIModuloServiceApi();
			var moduloRequest = new CIAC_TAS_Service.Contracts.V1.Requests.UpdateModuloRequest
			{
				Nombre = ModuloModelView.Nombre,
				ModuloCodigo = ModuloModelView.ModuloCodigo,
			};

			var moduloResponse = await moduloApi.UpdateAsync(id, moduloRequest);
			if (!moduloResponse.IsSuccessStatusCode)
			{
				var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(moduloResponse.Error.Content);

				Message = String.Join(" ", errorResponse.Errors.Select(x => x.Message));

				return Page();
			}

			return RedirectToPage("/General/Modulos");
		}

		private IModuloServiceApi GetIModuloServiceApi()
		{
			return RestService.For<IModuloServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
			{
				AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
			});
		}
	}
}
