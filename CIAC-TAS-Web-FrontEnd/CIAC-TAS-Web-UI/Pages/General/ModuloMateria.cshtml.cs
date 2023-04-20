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
    public class ModuloMateriaModel : PageModel
    {
		[BindProperty]
		public ModuloMateriaModelView ModuloMateriaModelView { get; set; }

		public List<SelectListItem> ModuloOptions { get; set; }
		public List<SelectListItem> MateriaOptions { get; set; }


		[TempData]
		public string Message { get; set; }

		private readonly IConfiguration _configuration;
		public ModuloMateriaModel(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public async Task OnGetNewModuloMateriaAsync()
		{
			var moduloApi = GetIModuloServiceApi();
			var moduloResponse = await moduloApi.GetAllAsync();


			if (moduloResponse.IsSuccessStatusCode)
			{
				ModuloOptions = moduloResponse.Content.Data.Select(x => new SelectListItem { Text = x.Nombre, Value = x.Id.ToString() }).ToList();
			}

			var materiaApi = GetIMateriaServiceApi();
			var materiaResponse = await materiaApi.GetAllAsync();

			if (materiaResponse.IsSuccessStatusCode)
			{
				MateriaOptions = materiaResponse.Content.Data.Select(x => new SelectListItem { Text = x.Nombre, Value = x.Id.ToString() }).ToList();
			}
		}

		public async Task<IActionResult> OnPostNewModuloMateriaAsync()
		{
			if (!ModelState.IsValid)
			{
				Message = "Por favor complete el formulario correctamente";

				var moduloApi = GetIModuloServiceApi();
				var moduloResponse = await moduloApi.GetAllAsync();


				if (moduloResponse.IsSuccessStatusCode)
				{
					ModuloOptions = moduloResponse.Content.Data.Select(x => new SelectListItem { Text = x.Nombre, Value = x.Id.ToString() }).ToList();
				}

				var materiaApi = GetIMateriaServiceApi();
				var materiaResponse = await materiaApi.GetAllAsync();

				if (materiaResponse.IsSuccessStatusCode)
				{
					MateriaOptions = materiaResponse.Content.Data.Select(x => new SelectListItem { Text = x.Nombre, Value = x.Id.ToString() }).ToList();
				}

				return Page();
			}

			var moduloMateriaApi = GetIModuloMateriaServiceApi();
			var moduloMateriaRequest = new CIAC_TAS_Service.Contracts.V1.Requests.CreateModuloMateriaRequest
			{
				ModuloId = ModuloMateriaModelView.ModuloId,
				MateriaId = ModuloMateriaModelView.MateriaId,
			};

			var moduloMateriaResponse = await moduloMateriaApi.CreateAsync(moduloMateriaRequest);
			if (!moduloMateriaResponse.IsSuccessStatusCode)
			{
				var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(moduloMateriaResponse.Error.Content);

				Message = String.Join(" ", errorResponse.Errors.Select(x => x.Message));

				var moduloApi = GetIModuloServiceApi();
				var moduloResponse = await moduloApi.GetAllAsync();


				if (moduloResponse.IsSuccessStatusCode)
				{
					ModuloOptions = moduloResponse.Content.Data.Select(x => new SelectListItem { Text = x.Nombre, Value = x.Id.ToString() }).ToList();
				}

				var materiaApi = GetIMateriaServiceApi();
				var materiaResponse = await materiaApi.GetAllAsync();

				if (materiaResponse.IsSuccessStatusCode)
				{
					MateriaOptions = materiaResponse.Content.Data.Select(x => new SelectListItem { Text = x.Nombre, Value = x.Id.ToString() }).ToList();
				}

				return Page();
			}

			return RedirectToPage("/General/ModuloMaterias");
		}

		private IModuloServiceApi GetIModuloServiceApi()
		{
			return RestService.For<IModuloServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
			{
				AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
			});
		}

		private IMateriaServiceApi GetIMateriaServiceApi()
		{
			return RestService.For<IMateriaServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
			{
				AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
			});
		}

		private IModuloMateriaServiceApi GetIModuloMateriaServiceApi()
		{
			return RestService.For<IModuloMateriaServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
			{
				AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
			});
		}
	}
}
