using CIAC_TAS_Service.Contracts.V1.Requests;
using CIAC_TAS_Service.Contracts.V1.Responses;
using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.Helper;
using CIAC_TAS_Web_UI.ModelViews.ASA;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Refit;
using static CIAC_TAS_Service.Contracts.V1.ApiRoute;

namespace CIAC_TAS_Web_UI.Pages.ASA
{
    public class ConfiguracionPreguntaAsaModel : PageModel
    {
        [BindProperty]
        public ConfiguracionPreguntaAsaModelView ConfiguracionPreguntaAsaModelView { get; set; }

        public List<SelectListItem> GrupoOptions { get; set; }

        [TempData]
        public string Message { get; set; }

        private readonly IConfiguration _configuration;
        public ConfiguracionPreguntaAsaModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task OnGetNewConfiguracionPreguntaAsaAsync()
        {
			ConfiguracionPreguntaAsaModelView = new ConfiguracionPreguntaAsaModelView();
            ConfiguracionPreguntaAsaModelView.FechaInicial = DateTime.Today;

            await FillGrupoOptions();
        }

        public async Task<IActionResult> OnPostNewConfiguracionPreguntaAsaAsync()
        {
            if (!ModelState.IsValid)
            {
                Message = "Por favor complete el formulario correctamente";

                await FillGrupoOptions();

                return Page();
            }            

            var configuracionPreguntaAsaServiceApi = GetIConfiguracionPreguntaAsaServiceApi();

            var createConfiguracionPreguntaAsaRequest = new CreateConfiguracionPreguntaAsaRequest
            {
                GrupoId = ConfiguracionPreguntaAsaModelView.GrupoId,
                CantitdadPreguntas = ICuestionarioASAHelper.NUMERO_PREGUNTAS_DEFAULT,
                FechaInicial = ConfiguracionPreguntaAsaModelView.FechaInicial,
                FechaFin = ConfiguracionPreguntaAsaModelView.FechaInicial.AddHours(1).AddMinutes(40)
			};

            var createConfiguracionPreguntaAsaResponse = await configuracionPreguntaAsaServiceApi.CreateAsync(createConfiguracionPreguntaAsaRequest);
            if (!createConfiguracionPreguntaAsaResponse.IsSuccessStatusCode)
            {
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(createConfiguracionPreguntaAsaResponse.Error.Content);

                Message = String.Join(" ", errorResponse.Errors.Select(x => x.Message));

                await FillGrupoOptions();

                return Page();
            }

            return RedirectToPage("/ASA/ConfiguracionPreguntasAsa");
        }

        public async Task<IActionResult> OnGetEditConfiguracionPreguntaAsaAsync(int id)
        {
            var configuracionPreguntaAsaServiceApi = GetIConfiguracionPreguntaAsaServiceApi();
            var createConfiguracionPreguntaAsaResponse = await configuracionPreguntaAsaServiceApi.GetAsync(id);

            if (!createConfiguracionPreguntaAsaResponse.IsSuccessStatusCode)
            {
                Message = "Ocurrio un error inesperado";

                return RedirectToPage("/ASA/ConfiguracionPreguntasAsa");
            }

            await FillGrupoOptions();

            var configuracionPreguntaAsa = createConfiguracionPreguntaAsaResponse.Content;
            ConfiguracionPreguntaAsaModelView = new ConfiguracionPreguntaAsaModelView
            {
                Id = configuracionPreguntaAsa.Id,
                GrupoId = configuracionPreguntaAsa.GrupoId,
				CantidadPreguntas = configuracionPreguntaAsa.CantitdadPreguntas,
                FechaInicial = configuracionPreguntaAsa.FechaInicial,
                FechaFin = configuracionPreguntaAsa.FechaFin
            };

            return Page();
        }

        public async Task<IActionResult> OnPostEditConfiguracionPreguntaAsaAsync(int id)
        {
            if (!ModelState.IsValid)
            {
                Message = "Por favor complete el formulario correctamente";

                await FillGrupoOptions();

                return Page();
            }

            var configuracionPreguntaAsaServiceApi = GetIConfiguracionPreguntaAsaServiceApi();
            var configuracionPreguntaAsaRequest = new UpdateConfiguracionPreguntaAsaRequest
            {
                GrupoId = ConfiguracionPreguntaAsaModelView.GrupoId,
                CantitdadPreguntas = ICuestionarioASAHelper.NUMERO_PREGUNTAS_DEFAULT,
                FechaInicial = ConfiguracionPreguntaAsaModelView.FechaInicial,
                FechaFin = ConfiguracionPreguntaAsaModelView.FechaInicial.AddHours(1).AddMinutes(40)
			};

            var configuracionPreguntaAsaResponse = await configuracionPreguntaAsaServiceApi.UpdateAsync(id, configuracionPreguntaAsaRequest);
            if (!configuracionPreguntaAsaResponse.IsSuccessStatusCode)
            {
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(configuracionPreguntaAsaResponse.Error.Content);

                Message = String.Join(" ", errorResponse.Errors.Select(x => x.Message));

                await FillGrupoOptions();

                return Page();
            }

            return RedirectToPage("/ASA/ConfiguracionPreguntasAsa");
        }

        private async Task FillGrupoOptions()
        {
            var grupoServiceApi = GetIGrupoServiceApiServiceApi();
            var grupoResponse = await grupoServiceApi.GetAllAsync();

            if (grupoResponse.IsSuccessStatusCode)
            {
                GrupoOptions = grupoResponse.Content.Data.Select(x => new SelectListItem { Text = x.Nombre, Value = x.Id.ToString() }).ToList();
            }
        }

        private IGrupoServiceApi GetIGrupoServiceApiServiceApi()
        {
            return RestService.For<IGrupoServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
        }

        private IConfiguracionPreguntaAsaServiceApi GetIConfiguracionPreguntaAsaServiceApi()
        {
            return RestService.For<IConfiguracionPreguntaAsaServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
        }
    }
}
