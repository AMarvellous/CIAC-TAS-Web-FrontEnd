using AspNetCore.Reporting;
using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.Helper;
using CIAC_TAS_Web_UI.ModelViews.ASA;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Refit;
using System.Data;
using System.Text;

namespace CIAC_TAS_Web_UI.Pages.ASA
{
    public class GraficaEstudianteModel : PageModel
    {
		[BindProperty]
		public IEnumerable<GraficaEstudianteModelView> GraficaEstudianteModelView { get; set; } = new List<GraficaEstudianteModelView>();

		[BindProperty]
		public string UserIdWhenAdmin { get; set; } = string.Empty;

		[TempData]
		public string Message { get; set; }

		private readonly IConfiguration _configuration;
		private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment;
		public GraficaEstudianteModel(IConfiguration configuration, Microsoft.AspNetCore.Hosting.IHostingEnvironment environment)
		{
			_configuration = configuration;
			_environment = environment;
		}

		public async Task<IActionResult> OnGetViewGraficaEstudianteAsync(Guid loteRespuestaId, string externalUserId = null)
        {
			var respuestasAsaConsolidadoServiceApi = RestService.For<IRespuestasAsaconsolidadoServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
			{
				AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
			});

			var userId = HttpContext.Session.GetString(Session.SessionUserId);

			if (externalUserId != null && externalUserId != string.Empty)
			{
				userId = externalUserId;
				UserIdWhenAdmin = externalUserId;
			}

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

		public async Task<IActionResult> OnGetDownloadGraficaEstudianteReportAsync(Guid loteRespuestaId, string externalUserId = null)
		{
			string renderFormart = "PDF";
			string mimetype = "";
			int extension = 1;
			string reportPath = Path.Combine(_environment.WebRootPath, "Reports/ASA/ReportExample.rdlc");
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

			var respuestasAsaConsolidadoServiceApi = RestService.For<IRespuestasAsaconsolidadoServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
			{
				AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
			});

			var userId = HttpContext.Session.GetString(Session.SessionUserId);
			if (externalUserId != null && externalUserId != string.Empty)
			{
				userId = externalUserId;
				UserIdWhenAdmin = externalUserId;
			}

			var respuestasAsaConsolidadoResponse = await respuestasAsaConsolidadoServiceApi.GetAllByUserIdAndLoteAsync(loteRespuestaId, userId);

			if (!respuestasAsaConsolidadoResponse.IsSuccessStatusCode)
			{
				Message = "Ocurrio un error inesperado";

				return Page();
			}

			var respuestasAsaConsolidado = respuestasAsaConsolidadoResponse.Content.Data;

			var estudianteServiceApi = RestService.For<IEstudianteServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
			{
				AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
			});

			var estudianteResponse = await estudianteServiceApi.GetByUserIdAsync(userId);

			if (!estudianteResponse.IsSuccessStatusCode)
			{
				Message = "Ocurrio un error inesperado";

				return Page();
			}

			var estudiante = estudianteResponse.Content;

			Dictionary<string, string> parameters = new Dictionary<string, string>();
			parameters.Add("EstudianteNombre", estudiante.Nombre + " " + estudiante.ApellidoPaterno + " " + estudiante.ApellidoMaterno);
			parameters.Add("FechaExamen", respuestasAsaConsolidado.FirstOrDefault()?.ConfiguracionPreguntaAsaResponse.FechaInicial.ToString("dd-MM-yyyy"));

			DataTable dataTable = new DataTable();
			dataTable.Columns.Add("NumeroPregunta");
			dataTable.Columns.Add("PreguntaTexto");
			dataTable.Columns.Add("RespuestaTexto");
			dataTable.Columns.Add("RespuestaCorrectaTexto");

			foreach (var item in respuestasAsaConsolidado)
			{
				DataRow dataRow = dataTable.NewRow();
				dataRow["NumeroPregunta"] = item.NumeroPregunta;
				dataRow["PreguntaTexto"] = item.PreguntaTexto;
				dataRow["RespuestaTexto"] = item.RespuestaTexto;
				dataRow["RespuestaCorrectaTexto"] = item.RespuestaCorrecta ? "Correcto" : "Incorrecto";

				dataTable.Rows.Add(dataRow);
			}

			var report = new LocalReport(reportPath);
			report.AddDataSource("dsCuestionarioASA", dataTable);

			var result = report.Execute(RenderType.Pdf, extension, parameters, mimetype);

			return File(result.MainStream, "application/pdf", "ReporteEstudianteCuestionarioASA_" + DateTime.Now.Ticks + ".pdf");
		}

	}
}
