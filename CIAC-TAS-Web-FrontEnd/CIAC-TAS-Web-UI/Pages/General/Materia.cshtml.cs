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
    public class MateriaModel : PageModel
    {
		[BindProperty]
		public MateriaModelView MateriaModelView { get; set; }

		[TempData]
		public string Message { get; set; }

		private readonly IConfiguration _configuration;
		public MateriaModel(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public async Task OnGetNewMateriaAsync()
		{

		}

		public async Task<IActionResult> OnPostNewMateriaAsync()
		{
			if (!ModelState.IsValid)
			{
				Message = "Por favor complete el formulario correctamente";

				return Page();
			}

			var materiaApi = GetIMateriaServiceApi();
			var materiaRequest = new CIAC_TAS_Service.Contracts.V1.Requests.CreateMateriaRequest
			{
				Nombre = MateriaModelView.Nombre,
				MateriaCodigo = MateriaModelView.MateriaCodigo
            };

			var materiaResponse = await materiaApi.CreateAsync(materiaRequest);
			if (!materiaResponse.IsSuccessStatusCode)
			{
				var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(materiaResponse.Error.Content);

				Message = String.Join(" ", errorResponse.Errors.Select(x => x.Message));

				return Page();
			}

			return RedirectToPage("/General/Materias");
		}

		public async Task<IActionResult> OnGetEditMateriaAsync(int id)
		{
			var materiaApi = GetIMateriaServiceApi();
			var materiaResponse = await materiaApi.GetAsync(id);

			if (!materiaResponse.IsSuccessStatusCode)
			{
				Message = "Ocurrio un error inesperado";

				return RedirectToPage("/General/Materias");
			}

			var materia = materiaResponse.Content;
			MateriaModelView = new MateriaModelView
			{
				Id = materia.Id,
				Nombre = materia.Nombre,
				MateriaCodigo = materia.MateriaCodigo
			};

			return Page();
		}
		public async Task<IActionResult> OnPostEditMateriaAsync(int id)
		{
			if (!ModelState.IsValid)
			{
				Message = "Por favor complete el formulario correctamente";

				return Page();
			}

			var materiaApi = GetIMateriaServiceApi();
			var materiaRequest = new CIAC_TAS_Service.Contracts.V1.Requests.UpdateMateriaRequest
			{
				Nombre = MateriaModelView.Nombre,
				MateriaCodigo = MateriaModelView.MateriaCodigo
            };

			var materiaResponse = await materiaApi.UpdateAsync(id, materiaRequest);
			if (!materiaResponse.IsSuccessStatusCode)
			{
				var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(materiaResponse.Error.Content);

				Message = String.Join(" ", errorResponse.Errors.Select(x => x.Message));

				return Page();
			}

			return RedirectToPage("/General/Materias");
		}

		private IMateriaServiceApi GetIMateriaServiceApi()
		{
			return RestService.For<IMateriaServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
			{
				AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
			});
		}
	}
}
