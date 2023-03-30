using AspNetCore.Reporting;
using CIAC_TAS_Service.Contracts.V1.Responses;
using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.Helper;
using CIAC_TAS_Web_UI.ModelViews.ASA;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Refit;
using System.Data;
using System.Text;
using static CIAC_TAS_Service.Contracts.V1.ApiRoute;

namespace CIAC_TAS_Web_UI.Pages.ASA
{
    public class GenerarExamenAsaModel : PageModel
    {
        [BindProperty]
        public IEnumerable<ExamenGeneradoModelView> ExamenGeneradoModelView { get; set; } = new List<ExamenGeneradoModelView>();

        [TempData]
        public string Message { get; set; }

        private readonly IConfiguration _configuration;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment;
        public GenerarExamenAsaModel(IConfiguration configuration, Microsoft.AspNetCore.Hosting.IHostingEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var examenGeneradoServiceApi = RestService.For<IExamenGeneradoServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
            var examenGeneradoResponse = await examenGeneradoServiceApi.GetExamenHeadersAsync();

            if (!examenGeneradoResponse.IsSuccessStatusCode)
            {              
                Message = "Ocurrio un error inesperado";

                return Page();
            }

            var examenGenerado = examenGeneradoResponse.Content.Data;
            ExamenGeneradoModelView = examenGenerado.Select(x => new ModelViews.ASA.ExamenGeneradoModelView
            {
                GrupoId = x.GrupoId,
                GrupoNombre = x.GrupoResponse.Nombre,
                Fecha = x.Fecha,
                ExamenGeneradoGuid = x.ExamenGeneradoGuid,
            });

            return Page();
        }

        public async Task<IActionResult> OnGetDownloadConfiguracionPreguntaAsaAsync(int grupoId, Guid examenGeneradoGuid, string grupoNombre)
        {        
            var examenGeneradoServiceApi = RestService.For<IExamenGeneradoServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });

            var examenGeneradoResponse = await examenGeneradoServiceApi.GetExamenByGrupoGuidAsync(grupoId, examenGeneradoGuid);
            if (!examenGeneradoResponse.IsSuccessStatusCode)
            {
                Message = "Ocurrio un error inesperado";

                return Page();
            }

            var examenesGeneradosData = examenGeneradoResponse.Content.Data;

            return GetExamenGeneradoFileContentResult(examenesGeneradosData.ToList(), grupoNombre);
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
    }
}
