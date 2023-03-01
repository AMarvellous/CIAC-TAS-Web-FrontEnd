using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.Helper;
using CIAC_TAS_Web_UI.ModelViews.ASA;
using CIAC_TAS_Web_UI.ModelViews.Estudiante;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Refit;

namespace CIAC_TAS_Web_UI.Pages.ASA
{
    public class ConfiguracionPreguntasAsaModel : PageModel
    {
        [BindProperty]
        public IEnumerable<ConfiguracionPreguntaAsaModelView> ConfiguracionPreguntaAsaModelView { get; set; } = new List<ConfiguracionPreguntaAsaModelView>();

        [TempData]
        public string Message { get; set; }

        private readonly IConfiguration _configuration;
        public ConfiguracionPreguntasAsaModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var configuracionPreguntaAsaServiceApi = RestService.For<IConfiguracionPreguntaAsaServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
            var configuracionPreguntaAsaResponse = await configuracionPreguntaAsaServiceApi.GetAllAsync();

            if (!configuracionPreguntaAsaResponse.IsSuccessStatusCode)
            {
                Message = "Ocurrio un error inesperado";

                return Page();
            }

            var configuracionPreguntasAsa = configuracionPreguntaAsaResponse.Content.Data;
            ConfiguracionPreguntaAsaModelView = configuracionPreguntasAsa.Select(x => new ConfiguracionPreguntaAsaModelView
            {
                Id = x.Id,
                GrupoModelView = new ModelViews.General.GrupoModelView { Nombre = x.GrupoResponse.Nombre },
                CantidadPreguntas = x.CantitdadPreguntas,
                FechaInicial = x.FechaInicial,
                FechaFin = x.FechaFin,
            });

            return Page();
        }    
    }
}
