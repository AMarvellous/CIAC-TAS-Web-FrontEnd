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
using static CIAC_TAS_Service.Contracts.V1.ApiRoute;

namespace CIAC_TAS_Web_UI.Pages.Estudiante
{
	public class EstudianteGrupoModel : PageModel
	{
		[BindProperty]
		public EstudianteGrupoModelView EstudianteGrupoModelView { get; set; }
		public List<EstudianteGrupoModelView> EstudianteGrupoModelViewList { get; set; } = new List<EstudianteGrupoModelView>();

        public List<SelectListItem> EstudiantesOptions { get; set; }
		public bool IsNewPage { get; set; }
        public string GrupoNombre { get; set; } = string.Empty;

		[TempData]
		public string Message { get; set; }

		private readonly IConfiguration _configuration;
		public EstudianteGrupoModel(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public async Task OnGetNewEstudianteGrupoAsync(int grupoId)
		{
            IsNewPage = true;
            EstudianteGrupoModelView = new EstudianteGrupoModelView();
			EstudianteGrupoModelView.GrupoId = grupoId;

            var grupoServiceApi = GetIGrupoServiceApi();
            var grupoResponse = await grupoServiceApi.GetAsync(grupoId);

            if (grupoResponse.IsSuccessStatusCode)
            {
                GrupoNombre = grupoResponse.Content.Nombre;
            }

            await FillEstudiantesOptions();
		}

		public async Task<IActionResult> OnPostNewEstudianteGrupoAsync()
		{
			IsNewPage = true;

            var grupoServiceApi = GetIGrupoServiceApi();
            var grupoResponse = await grupoServiceApi.GetAsync(EstudianteGrupoModelView.GrupoId);

            if (grupoResponse.IsSuccessStatusCode)
            {
                GrupoNombre = grupoResponse.Content.Nombre;
            }

            if (EstudianteGrupoModelView.EstudiantesIds.Count == 0 || EstudianteGrupoModelView.GrupoId == 0)
			{
				Message = "Por favor complete el formulario correctamente";

				await FillEstudiantesOptions();

				return Page();
			}

			var estudiantegrupoServiceApi = GetIEstudianteGrupoServiceApi();
			List<CreateEstudianteGrupoRequest> createEstudianteGrupoRequests = new List<CreateEstudianteGrupoRequest>();

			EstudianteGrupoModelView.EstudiantesIds.ForEach(estudianteId =>
			{
				createEstudianteGrupoRequests.Add(new CreateEstudianteGrupoRequest
				{
					EstudianteId = estudianteId,
					GrupoId = EstudianteGrupoModelView.GrupoId
                });
            });

			var estudiantePreguntaResponse = await estudiantegrupoServiceApi.CreateBatchAsync(createEstudianteGrupoRequests);
			if (!estudiantePreguntaResponse.IsSuccessStatusCode)
			{
				var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(estudiantePreguntaResponse.Error.Content);

				Message = String.Join(" ", errorResponse.Errors.Select(x => x.Message));

				await FillEstudiantesOptions();

				return Page();
			}

			return RedirectToPage("/Estudiante/EstudiantesGrupos");
		}

        public async Task<IActionResult> OnGetEstudianteGrupoAddAsync(int grupoIdParam)
        {
            IsNewPage = false;
            EstudianteGrupoModelView = new EstudianteGrupoModelView();
            EstudianteGrupoModelView.GrupoId = grupoIdParam;

            await FillEstudiantesOptionsNotAssigned(grupoIdParam);
            var estudiantegrupoServiceApi = GetIEstudianteGrupoServiceApi();
            var estudianteGrupoResponse = await estudiantegrupoServiceApi.GetAllByGrupoIdAsync(grupoIdParam);

            if (!estudianteGrupoResponse.IsSuccessStatusCode)
            {
                return RedirectToPage("/Estudiante/EstudianteGrupoEdit", "EditEstudianteGrupo", new { grupoId = grupoIdParam });
            }

			var estudianteGrupos = estudianteGrupoResponse.Content.Data;

			foreach (var estudianteGrupo in estudianteGrupos)
			{
				EstudianteGrupoModelViewList.Add(new EstudianteGrupoModelView
				{
					EstudianteId = estudianteGrupo.EstudianteId,
					GrupoId = estudianteGrupo.GrupoId,
					EstudianteNombre = estudianteGrupo.EstudianteResponse.Nombre + " " + estudianteGrupo.EstudianteResponse.ApellidoPaterno,
					GrupoNombre = estudianteGrupo.GrupoResponse.Nombre,
                });

            }

            var grupoServiceApi = GetIGrupoServiceApi();
            var grupoResponse = await grupoServiceApi.GetAsync(grupoIdParam);

            if (grupoResponse.IsSuccessStatusCode)
            {
                GrupoNombre = grupoResponse.Content.Nombre;
            }            

            return Page();
        }

        public async Task<IActionResult> OnPostEstudianteGrupoAddAsync()
        {
            IsNewPage = false;
            var grupoServiceApi = GetIGrupoServiceApi();            

            if (EstudianteGrupoModelView.EstudiantesIds.Count == 0 || EstudianteGrupoModelView.GrupoId == 0)
            {
                Message = "Por favor complete el formulario correctamente";

                var grupoResponse = await grupoServiceApi.GetAsync(EstudianteGrupoModelView.GrupoId);

                if (grupoResponse.IsSuccessStatusCode)
                {
                    GrupoNombre = grupoResponse.Content.Nombre;
                }

                await FillEstudiantesOptionsNotAssigned(EstudianteGrupoModelView.GrupoId);

                return Page();
            }

            var estudiantegrupoServiceApi = GetIEstudianteGrupoServiceApi();
            List<CreateEstudianteGrupoRequest> createEstudianteGrupoRequests = new List<CreateEstudianteGrupoRequest>();

            EstudianteGrupoModelView.EstudiantesIds.ForEach(estudianteId =>
            {
                createEstudianteGrupoRequests.Add(new CreateEstudianteGrupoRequest
                {
                    EstudianteId = estudianteId,
                    GrupoId = EstudianteGrupoModelView.GrupoId
                });
            });

            var estudiantePreguntaResponse = await estudiantegrupoServiceApi.CreateBatchAsync(createEstudianteGrupoRequests);
            if (!estudiantePreguntaResponse.IsSuccessStatusCode)
            {
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(estudiantePreguntaResponse.Error.Content);

                Message = String.Join(" ", errorResponse.Errors.Select(x => x.Message));

                var grupoResponse = await grupoServiceApi.GetAsync(EstudianteGrupoModelView.GrupoId);

                if (grupoResponse.IsSuccessStatusCode)
                {
                    GrupoNombre = grupoResponse.Content.Nombre;
                }

                await FillEstudiantesOptionsNotAssigned(EstudianteGrupoModelView.GrupoId);

                return Page();
            }

            return RedirectToPage("/Estudiante/EstudianteGrupoEdit", "EditEstudianteGrupo", new { grupoId = EstudianteGrupoModelView.GrupoId });
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

        private async Task FillEstudiantesOptions()
		{
			var estudianteServiceApi = GetIEstudianteServiceApi();
			var estudianteResponse = await estudianteServiceApi.GetAllAsync();

			if (estudianteResponse.IsSuccessStatusCode)
			{
				EstudiantesOptions = estudianteResponse.Content.Data.Select(x => new SelectListItem { Text = x.Nombre + " " + x.ApellidoPaterno + " " + x.ApellidoMaterno, Value = x.Id.ToString() }).ToList();
			}
		}

        private async Task FillEstudiantesOptionsNotAssigned(int grupoId)
        {
            var estudianteServiceApi = GetIEstudianteServiceApi();
            var estudianteResponse = await estudianteServiceApi.GetAllNotAssignedToGrupoAsync(grupoId);

            if (estudianteResponse.IsSuccessStatusCode)
            {
                EstudiantesOptions = estudianteResponse.Content.Data.Select(x => new SelectListItem { Text = x.Nombre + " " + x.ApellidoPaterno + " " + x.ApellidoMaterno, Value = x.Id.ToString() }).ToList();
            }
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

        private IGrupoServiceApi GetIGrupoServiceApi()
        {
            return RestService.For<IGrupoServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
        }
    }
}
