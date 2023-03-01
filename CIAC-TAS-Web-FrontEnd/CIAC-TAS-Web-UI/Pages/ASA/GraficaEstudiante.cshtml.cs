using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.Helper;
using CIAC_TAS_Web_UI.ModelViews.ASA;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Refit;

namespace CIAC_TAS_Web_UI.Pages.ASA
{
    public class GraficaEstudianteModel : PageModel
    {
		[BindProperty]
		public IEnumerable<GraficaEstudianteModelView> GraficaEstudianteModelView { get; set; } = new List<GraficaEstudianteModelView>();

		[TempData]
		public string Message { get; set; }

		private readonly IConfiguration _configuration;
		public GraficaEstudianteModel(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public async Task<IActionResult> OnGetViewGraficaEstudianteAsync(Guid loteRespuestaId)
        {
			var respuestasAsaConsolidadoServiceApi = RestService.For<IRespuestasAsaconsolidadoServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
			{
				AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
			});

			var userId = HttpContext.Session.GetString(Session.SessionUserId);
			var respuestasAsaConsolidadoResponse = await respuestasAsaConsolidadoServiceApi.GetAllByUserIdAndLoteAsync(loteRespuestaId, userId);

			if (!respuestasAsaConsolidadoResponse.IsSuccessStatusCode)
			{
				Message = "Ocurrio un error inesperado";

				return Page();
			}

			var respuestasAsaConsolidadosLista = respuestasAsaConsolidadoResponse.Content.Data;
			GraficaEstudianteModelView = respuestasAsaConsolidadosLista.Select(x => new GraficaEstudianteModelView
			{
				Id = x.Id,
				LoteRespuestasId = x.LoteRespuestasId,
				UserId = x.UserId,
				NumeroPregunta = x.NumeroPregunta,
				PreguntaTexto = x.PreguntaTexto,
				FechaLote = x.FechaLote,
				Opcion = x.Opcion,
				RespuestaTexto = x.RespuestaTexto,
				RespuestaCorrecta = x.RespuestaCorrecta,
				EsExamen = x.EsExamen,
			});

			return Page();
        }
    }
}
