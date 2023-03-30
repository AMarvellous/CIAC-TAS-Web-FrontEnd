using AspNetCore.Reporting;
using CIAC_TAS_Service.Contracts.V1.Responses;
using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.Helper;
using CIAC_TAS_Web_UI.ModelViews.ASA;
using CIAC_TAS_Web_UI.ModelViews.General;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Refit;
using System.Data;
using System.Text;

namespace CIAC_TAS_Web_UI.Pages.ASA
{
    public class GenerarExamenAsaDetalleModel : PageModel
    {
        [BindProperty]
        public int GrupoIdModelView { get; set; }
        public List<SelectListItem> GrupoOptions { get; set; } = new List<SelectListItem>();

        [TempData]
        public string Message { get; set; }

        private readonly IConfiguration _configuration;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment;
        public GenerarExamenAsaDetalleModel(IConfiguration configuration, Microsoft.AspNetCore.Hosting.IHostingEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        public async Task OnGetNewGenerarExamenAsaAsync()
        {
            await FillGrupoOptions();
        }

        public async Task<IActionResult> OnGetGenerarExamenAsaAsync(int grupoId)
        {
            var grupoServiceApi = GetIGrupoServiceApiServiceApi();
            var grupoResponse = await grupoServiceApi.GetAllAsync();

            if (grupoResponse.IsSuccessStatusCode)
            {
                GrupoOptions = grupoResponse.Content.Data.Select(x => new SelectListItem { Text = x.Nombre, Value = x.Id.ToString() }).ToList();
            }

            if (grupoId == 0)
            {
                Message = "Seleccione un Grupo";               

                return Page();
            }

            var examenGeneradoServiceApi = RestService.For<IExamenGeneradoServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });

            var examenGeneradoServiceResponse = await examenGeneradoServiceApi.CreatePreguntasExamenGeneradoAsync(grupoId, ICuestionarioASAHelper.NUMERO_PREGUNTAS_EXAMEN_DEFAULT);
            if (!examenGeneradoServiceResponse.IsSuccessStatusCode)
            {
                Message = "Ocurrio un error al generar el examen, vuelva a intentarlo";

                return Page();
            }

            var examenesGeneradosData = examenGeneradoServiceResponse.Content;
            var grupoNombre = grupoResponse.Content.Data
                .Where(x => x.Id == grupoId)
                .Select(x => x.Nombre)
                .FirstOrDefault();

            return GetExamenGeneradoFileContentResult(examenesGeneradosData, grupoNombre);
        }

        private FileContentResult GetExamenGeneradoFileContentResult(List<ExamenGeneradoResponse> examenesGeneradosData, string grupoNombre)
        {
            string mimetype = "";
            int extension = 1;
            string reportPath = Path.Combine(_environment.WebRootPath, "Reports/ASA/ReportExamenGenerado.rdlc");
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("TituloReporte", "Examen ASA");
            parameters.Add("GrupoNombre", grupoNombre);

            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("GrupoId");
            dataTable.Columns.Add("Fecha");
            dataTable.Columns.Add("ExamenGeneradoGuid");
            dataTable.Columns.Add("NumeroOpcion");
            dataTable.Columns.Add("NumeroPregunta");
            dataTable.Columns.Add("OpcionTexto");
            dataTable.Columns.Add("PreguntaTexto");

            foreach (var item in examenesGeneradosData)
            {
                DataRow dataRow = dataTable.NewRow();
                dataRow["GrupoId"] = item.GrupoId;
                dataRow["Fecha"] = item.Fecha;
                dataRow["ExamenGeneradoGuid"] = item.ExamenGeneradoGuid;
                dataRow["NumeroOpcion"] = item.NumeroOpcion;
                dataRow["NumeroPregunta"] = item.NumeroPregunta;
                dataRow["OpcionTexto"] = item.OpcionTexto;
                dataRow["PreguntaTexto"] = item.PreguntaTexto;

                dataTable.Rows.Add(dataRow);
            }

            var report = new LocalReport(reportPath);
            report.AddDataSource("dsExamenGenerado", dataTable);

            var result = report.Execute(RenderType.Pdf, extension, parameters, mimetype);

            return File(result.MainStream, "application/pdf", "ExamenASAGenerado_" + grupoNombre + "_" + DateTime.Now.Ticks + ".pdf");
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
    }
}
