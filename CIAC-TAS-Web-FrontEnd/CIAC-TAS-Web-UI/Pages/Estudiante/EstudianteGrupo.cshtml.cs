using CIAC_TAS_Service.Contracts.V1.Requests;
using CIAC_TAS_Service.Contracts.V1.Responses;
using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.Helper;
using CIAC_TAS_Web_UI.ModelViews.Estudiante;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Refit;

namespace CIAC_TAS_Web_UI.Pages.Estudiante
{
    public class EstudianteGrupoModel : PageModel
    {
		[BindProperty]
		public EstudianteGrupoModelView EstudianteGrupoModelView { get; set; }

		public List<SelectListItem> EstudiantesOptions { get; set; }
		public List<SelectListItem> GrupoOptions { get; set; }

		[TempData]
		public string Message { get; set; }

		private readonly IConfiguration _configuration;
		public EstudianteGrupoModel(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public async Task OnGetNewEstudianteGrupoAsync()
		{
			await FillEstudiantesOptions();
			await FillGrupoOptions();
		}

		public async Task<IActionResult> OnPostNewEstudianteGrupoAsync()
		{
			if (!ModelState.IsValid)
			{
				Message = "Por favor complete el formulario correctamente";

				await FillEstudiantesOptions();
				await FillGrupoOptions();

				return Page();
			}

			var estudiantegrupoServiceApi = GetIEstudianteGrupoServiceApi();

			var estudiantegrupoRequest = new CreateEstudianteGrupoRequest
			{
				EstudianteId = EstudianteGrupoModelView.EstudianteId,
				GrupoId = EstudianteGrupoModelView.GrupoId
			};

			var estudiantePreguntaResponse = await estudiantegrupoServiceApi.CreateAsync(estudiantegrupoRequest);
			if (!estudiantePreguntaResponse.IsSuccessStatusCode)
			{
				var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(estudiantePreguntaResponse.Error.Content);

				Message = String.Join(" ", errorResponse.Errors.Select(x => x.Message));

				await FillEstudiantesOptions();
				await FillGrupoOptions();

				return Page();
			}

			return RedirectToPage("/Estudiante/EstudiantesGrupos");
		}

		//public async Task<IActionResult> OnGetEditEstudianteGrupoAsync(int estudianteId, int grupoId)
		//{
		//	var estudiantegrupoServiceApi = GetIEstudianteGrupoServiceApi();
		//	var estudianteGrupoResponse = await estudiantegrupoServiceApi.GetAsync(estudianteId, grupoId);

		//	if (!estudianteGrupoResponse.IsSuccessStatusCode)
		//	{
		//		Message = "Ocurrio un error inesperado";

		//		return RedirectToPage("/General/EstudiantesGrupos");
		//	}

		//	await FillEstudiantesOptions();
		//	await FillGrupoOptions();

		//	var estudianteGrupo = estudianteGrupoResponse.Content;
		//	EstudianteGrupoModelView = new EstudianteGrupoModelView
		//	{
		//		EstudianteId = estudianteGrupo.EstudianteId,
		//		GrupoId = estudianteGrupo.GrupoId
		//	};

		//	return Page();
		//}

		//public async Task<IActionResult> OnPostEditEstudianteGrupoAsync(int estudianteId, int grupoId)
		//{
		//	if (!ModelState.IsValid)
		//	{
		//		Message = "Por favor complete el formulario correctamente";

		//		await FillEstudiantesOptions();
		//		await FillGrupoOptions();

		//		return Page();
		//	}

		//	var estudiantegrupoServiceApi = GetIEstudianteGrupoServiceApi();
		//	var configuracionPreguntaAsaRequest = new UpdateConfiguracionPreguntaAsaRequest
		//	{
		//		GrupoId = ConfiguracionPreguntaAsaModelView.GrupoId,
		//		CantitdadPreguntas = CuestionarioASAHelper.NUMERO_PREGUNTAS_DEFAULT,
		//		FechaInicial = ConfiguracionPreguntaAsaModelView.FechaInicial,
		//		FechaFin = ConfiguracionPreguntaAsaModelView.FechaInicial.AddHours(1)
		//	};

		//	var configuracionPreguntaAsaResponse = await estudiantegrupoServiceApi.(id, configuracionPreguntaAsaRequest);
		//	if (!configuracionPreguntaAsaResponse.IsSuccessStatusCode)
		//	{
		//		var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(configuracionPreguntaAsaResponse.Error.Content);

		//		Message = String.Join(" ", errorResponse.Errors.Select(x => x.Message));

		//		await FillGrupoOptions();

		//		return Page();
		//	}

		//	return RedirectToPage("/ASA/ConfiguracionPreguntasAsa");
		//}

		private async Task FillGrupoOptions()
		{
			var grupoServiceApi = GetIGrupoServiceApiServiceApi();
			var grupoResponse = await grupoServiceApi.GetAllAsync();

			if (grupoResponse.IsSuccessStatusCode)
			{
				GrupoOptions = grupoResponse.Content.Data.Select(x => new SelectListItem { Text = x.Nombre, Value = x.Id.ToString() }).ToList();
			}
		}

		private async Task FillEstudiantesOptions()
		{
			var estudianteGrupoServiceApi = GetIEstudianteServiceApi();
			var estudianteGrupoResponse = await estudianteGrupoServiceApi.GetAllAsync();

			if (estudianteGrupoResponse.IsSuccessStatusCode)
			{
				EstudiantesOptions = estudianteGrupoResponse.Content.Data.Select(x => new SelectListItem { Text = x.Nombre, Value = x.Id.ToString() }).ToList();
			}
		}

		private IGrupoServiceApi GetIGrupoServiceApiServiceApi()
		{
			return RestService.For<IGrupoServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
			{
				AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
			});
		}

		private IEstudianteGrupoServiceApi GetIEstudianteGrupoServiceApi()
		{
			return RestService.For<IEstudianteGrupoServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
			{
				AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
			});
		}

		private IEstudianteServiceApi GetIEstudianteServiceApi()
		{
			return RestService.For<IEstudianteServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
			{
				AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
			});
		}
	}
}
