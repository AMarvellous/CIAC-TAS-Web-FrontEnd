using AspNetCore.Reporting;
using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.Helper;
using CIAC_TAS_Web_UI.ModelViews.ASA;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Refit;
using System.Data;
using System.Text;
using static CIAC_TAS_Service.Contracts.V1.ApiRoute;

namespace CIAC_TAS_Web_UI.Pages.ASA
{
    public class GraficaEstudianteModel : PageModel
    {
		[BindProperty]
		public List<GraficaEstudianteModelView> GraficaEstudianteModelView { get; set; } = new List<GraficaEstudianteModelView>();

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
			foreach (var item in respuestasAsaConsolidadosLista)
			{
				GraficaEstudianteModelView.Add(new ModelViews.ASA.GraficaEstudianteModelView
				{
                    Id = item.Id,
                    LoteRespuestasId = item.LoteRespuestasId,
                    UserId = item.UserId,
                    NumeroPregunta = item.NumeroPregunta,
                    PreguntaTexto = item.PreguntaTexto,
                    FechaLote = item.FechaLote,
                    Opcion = item.Opcion,
                    RespuestaTexto = item.RespuestaTexto,
                    RespuestaCorrecta = item.RespuestaCorrecta,
                    EsExamen = item.EsExamen,
                    GrupoPreguntaAsaNombre = await GetGrupoPreguntaAsaNombreByPreguntaAsaIdAsync(item.NumeroPregunta)
                });
            }

            return Page();
        }

        private async Task<string> GetGrupoPreguntaAsaNombreByPreguntaAsaIdAsync(int numeroPregunta)
        {
            string grupoPreguntaAsaNombre = string.Empty;
            var preguntaAsaServiceApi = RestService.For<IPreguntaAsaServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });

            var preguntaAsaServiceResponse = await preguntaAsaServiceApi.GetByNumeroPreguntaAsync(numeroPregunta);
            if (preguntaAsaServiceResponse.IsSuccessStatusCode)
            {
                grupoPreguntaAsaNombre = preguntaAsaServiceResponse.Content.GrupoPreguntaAsaResponse.Nombre;
            }

            return grupoPreguntaAsaNombre;
        }

        public async Task<IActionResult> OnGetDownloadGraficaEstudianteReportAsync(Guid loteRespuestaId, string externalUserId = null)
		{
			string renderFormart = "PDF";
			string mimetype = "";
			int extension = 1;
			string reportPath = Path.Combine(_environment.WebRootPath, "Reports/ASA/ReporteExamenAsa.rdlc");
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
			var respuestasAsaConsolidadoFirstRow = respuestasAsaConsolidado.FirstOrDefault();
			var fechaExamen = DateTime.Today.ToString("dd-MM-yyyy");
			var tituloReporte = "Reporte de Practica ASA";

			if (respuestasAsaConsolidadoFirstRow.ConfiguracionPreguntaAsaResponse == null)
			{
				fechaExamen = respuestasAsaConsolidadoFirstRow.FechaLote.ToString("dd-MM-yyyy");
			} else
			{
				fechaExamen = respuestasAsaConsolidadoFirstRow.ConfiguracionPreguntaAsaResponse.FechaInicial.ToString("dd-MM-yyyy");
				tituloReporte = "Reporte del examen ASA";
			}			

			DataTable dataTable = new DataTable();
			dataTable.Columns.Add("NumeroPregunta");
			dataTable.Columns.Add("PreguntaTexto");
			dataTable.Columns.Add("RespuestaTexto");
			dataTable.Columns.Add("RespuestaCorrectaTexto");

            var numeroPreguntasCorrectas = 0;
			var numeroPreguntasIncorrectas = 0;
			var numeroPreguntasNoRespondidas = 0;
			var numeroPreguntasNota = 0;

            var respuestasCorrectasAirframe = 0;
            var respuestasIncorrectasAirframe = 0;
            var respuestasNoRespondidasAirframe = 0;

            var respuestasCorrectasGeneral = 0;
            var respuestasIncorrectasGeneral = 0;
            var respuestasNoRespondidasGeneral = 0;

            var respuestasCorrectasPowerPlant = 0;
            var respuestasIncorrectasPowerPlant = 0;
            var respuestasNoRespondidasPowerPlant = 0;

            foreach (var item in respuestasAsaConsolidado)
			{
				var respuestaCorrectaTexto = string.Empty;
				DataRow dataRow = dataTable.NewRow();
				dataRow["NumeroPregunta"] = item.NumeroPregunta;
				dataRow["PreguntaTexto"] = item.PreguntaTexto;
				dataRow["RespuestaTexto"] = item.RespuestaTexto;

				var grupoPreguntaAsaNombre = await GetGrupoPreguntaAsaNombreByPreguntaAsaIdAsync(item.NumeroPregunta);

                if (item.RespuestaCorrecta)
				{
					numeroPreguntasCorrectas++;
					respuestaCorrectaTexto = "Correcto";

					if (grupoPreguntaAsaNombre == "AIRFRAME")
					{
						respuestasCorrectasAirframe++;
                    } else if (grupoPreguntaAsaNombre == "GENERAL")
					{
						respuestasCorrectasGeneral++;
                    } else if (grupoPreguntaAsaNombre == "POWERPLANT")
                    {
                        respuestasCorrectasPowerPlant++;
                    }
                } else
				{
					respuestaCorrectaTexto = "Incorrecto";

					if (item.RespuestaTexto == string.Empty)
					{
						numeroPreguntasNoRespondidas++;
                        if (grupoPreguntaAsaNombre == "AIRFRAME")
                        {
                            respuestasNoRespondidasAirframe++;
                        }
                        else if (grupoPreguntaAsaNombre == "GENERAL")
                        {
                            respuestasNoRespondidasGeneral++;
                        }
                        else if (grupoPreguntaAsaNombre == "POWERPLANT")
                        {
                            respuestasNoRespondidasPowerPlant++;
                        }
                    } else
					{
						numeroPreguntasIncorrectas++;
                        if (grupoPreguntaAsaNombre == "AIRFRAME")
                        {
                            respuestasIncorrectasAirframe++;
                        }
                        else if (grupoPreguntaAsaNombre == "GENERAL")
                        {
                            respuestasIncorrectasGeneral++;
                        }
                        else if (grupoPreguntaAsaNombre == "POWERPLANT")
                        {
                            respuestasIncorrectasPowerPlant++;
                        }
                    }
				}

				dataRow["RespuestaCorrectaTexto"] = respuestaCorrectaTexto;

                dataTable.Rows.Add(dataRow);
			}

			numeroPreguntasNota = numeroPreguntasCorrectas;

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("EstudianteNombre", estudiante.Nombre + " " + estudiante.ApellidoPaterno + " " + estudiante.ApellidoMaterno);
            parameters.Add("FechaExamen", fechaExamen);
            parameters.Add("TituloReporte", tituloReporte);
            parameters.Add("PreguntasCorrectas", numeroPreguntasCorrectas.ToString());
			parameters.Add("PreguntasIncorrectas", numeroPreguntasIncorrectas.ToString());
			parameters.Add("PreguntasNoRespondidas", numeroPreguntasNoRespondidas.ToString());
			parameters.Add("NotaFinal", numeroPreguntasNota.ToString());

            parameters.Add("RespuestasCorrectasAirframe", respuestasCorrectasAirframe.ToString());
            parameters.Add("RespuestasIncorrectasAirframe", respuestasIncorrectasAirframe.ToString());
            parameters.Add("RespuestasNoRespondidasAirframe", respuestasNoRespondidasAirframe.ToString());

            parameters.Add("RespuestasCorrectasGeneral", respuestasCorrectasGeneral.ToString());
            parameters.Add("RespuestasIncorrectasGeneral", respuestasIncorrectasGeneral.ToString());
            parameters.Add("RespuestasNoRespondidasGeneral", respuestasNoRespondidasGeneral.ToString());

            parameters.Add("RespuestasCorrectasPowerPlant", respuestasCorrectasPowerPlant.ToString());
            parameters.Add("RespuestasIncorrectasPowerPlant", respuestasIncorrectasPowerPlant.ToString());
            parameters.Add("RespuestasNoRespondidasPowerPlant", respuestasNoRespondidasPowerPlant.ToString());            

            var report = new LocalReport(reportPath);
			report.AddDataSource("dsCuestionarioASA", dataTable);

			var result = report.Execute(RenderType.Pdf, extension, parameters, mimetype);

			return File(result.MainStream, "application/pdf", "ReporteEstudianteCuestionarioASA_" + DateTime.Now.Ticks + ".pdf");
		}

	}
}
