using AspNetCore.Reporting;
using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.Helper;
using CIAC_TAS_Web_UI.ModelViews.Estudiante;
using CIAC_TAS_Web_UI.ModelViews.Instructores;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Refit;
using static CIAC_TAS_Service.Contracts.V1.ApiRoute;
using System.Data;
using System.Text;
using CIAC_TAS_Service.Contracts.V1.Responses;

namespace CIAC_TAS_Web_UI.Pages.Estudiante
{
    public class AsistenciaEstudianteViewModel : PageModel
    {
        [TempData]
        public string Message { get; set; }
        [BindProperty]
        public List<EstudianteMateriaModelView> EstudianteMateriaModelView { get; set; } = new List<EstudianteMateriaModelView>();
        private readonly IConfiguration _configuration;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment;
        private readonly EstudianteSession _estudianteSession;
        public AsistenciaEstudianteViewModel(IConfiguration configuration, Microsoft.AspNetCore.Hosting.IHostingEnvironment environment, EstudianteSession estudianteSession)
        {
            _configuration = configuration;
            _estudianteSession = estudianteSession;
            _environment = environment;
        }

        public async Task OnGetAsync()
        {
            var userId = HttpContext.Session.GetString(Session.SessionUserId);
            var sessionToken = HttpContext.Session.GetString(Session.SessionToken);
            var estudianteId = await _estudianteSession.GetEstudianteIdByUserIdAsync(userId, sessionToken);

            var estudianteMateriaServiceApi = GetIEstudianteMateriaServiceApi();
            var estudianteMateriaResponse = await estudianteMateriaServiceApi.GetAllByEstudianteIdAsync(estudianteId);

            if (estudianteMateriaResponse.IsSuccessStatusCode)
            {
                EstudianteMateriaModelView = estudianteMateriaResponse.Content.Data.Select(x => new ModelViews.Estudiante.EstudianteMateriaModelView
                {
                    EstudianteId = x.EstudianteId,
                    GrupoId = x.GrupoId,
                    GrupoNombre = x.GrupoResponse.Nombre,
                    MateriaId = x.MateriaId,
                    MateriaNombre = x.MateriaResponse.Nombre
                }).ToList();
            }
        }

        public async Task<IActionResult> OnGetDownloadAsistenciaEstudianteReportByMateriaAsync(int grupoId, int materiaId)
        {
            var userId = HttpContext.Session.GetString(Session.SessionUserId);
            var sessionToken = HttpContext.Session.GetString(Session.SessionToken);
            var estudiante = await _estudianteSession.GetEstudianteByUserIdAsync(userId, sessionToken);

            if (estudiante == null)
            {
                Message = "Ocurrio un error inesperado";

                return RedirectToPage("/Estudiante/AsistenciaEstudianteView");
            }

            var asistenciaEstudianteHeaderServiceApi = GetIAsistenciaEstudianteHeaderServiceApi();
            var asistenciaEstudianteHeaderResponse = await asistenciaEstudianteHeaderServiceApi.GetAllHeadersByGrupoMateriaAndEstudianteIdAsync(grupoId, materiaId, estudiante.Id);
            
            if (!asistenciaEstudianteHeaderResponse.IsSuccessStatusCode)
            {
                Message = "Ocurrio un error inesperado";

                return RedirectToPage("/Estudiante/AsistenciaEstudianteView");
            }

            var grupoServiceApi = GetIGrupoServiceApi();
            var grupoResponse = await grupoServiceApi.GetAsync(grupoId);
            var grupoNombre = string.Empty;
            if (grupoResponse.IsSuccessStatusCode)
            {
                grupoNombre = grupoResponse.Content.Nombre;
            }

            var materiaServiceApi = GetIMateriaServiceApi();
            var materiaResponse = await materiaServiceApi.GetAsync(materiaId);
            var materiaNombre = string.Empty;
            if (materiaResponse.IsSuccessStatusCode)
            {
                materiaNombre = materiaResponse.Content.Nombre;
            }

            string renderFormart = "PDF";
            string mimetype = "";
            int extension = 1;
            string reportPath = Path.Combine(_environment.WebRootPath, "Reports/ASA/ReporteAsistenciaEstudianteIndividual.rdlc");
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("EstudianteId");
            dataTable.Columns.Add("EstudianteNombre");
            dataTable.Columns.Add("FechaAsistencia");
            dataTable.Columns.Add("Asistencia");
            dataTable.Columns.Add("AsistenciaPorcentaje");

            var asistenciaEstudianteHeaders = asistenciaEstudianteHeaderResponse.Content.Data;
            var asistenciaTotal = asistenciaEstudianteHeaders.Count();
            var asistenciaEstudianteTotales = 0;

            foreach (var asistenciaEstudianteDetail in asistenciaEstudianteHeaders)
            {
                DataRow dataRow = dataTable.NewRow();
                dataRow["EstudianteId"] = estudiante.Id;
                dataRow["EstudianteNombre"] = estudiante.Nombre + " " + estudiante.ApellidoPaterno;
                dataRow["FechaAsistencia"] = asistenciaEstudianteDetail.Fecha.ToString("dd-MM-yyyy");
                var asistencia = asistenciaEstudianteDetail.AsistenciaEstudiantesResponse
                    .Where(x => x.EstudianteId == estudiante.Id)
                    .Select(x => x.TipoAsistenciaResponse.Nombre)
                    .FirstOrDefault();
                dataRow["Asistencia"] = asistencia == null ? "No reportado" : asistencia;
                dataRow["AsistenciaPorcentaje"] = 0;
                dataTable.Rows.Add(dataRow);

                if (asistencia != null && (asistencia == "Justificada" || asistencia == "Presente"))
                {
                    asistenciaEstudianteTotales++;
                }
            }

            var rows = dataTable.Select("EstudianteId=" + estudiante.Id);
            var asistenciaFinal = asistenciaTotal == 0 ? 0 : (asistenciaEstudianteTotales * 100) / asistenciaTotal;
            foreach (DataRow row in rows)
            {
                row["AsistenciaPorcentaje"] = asistenciaFinal;
            }

            var report = new LocalReport(reportPath);
            report.AddDataSource("dsAsistenciaEstudiante", dataTable);

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("Fecha", DateTime.Today.ToString("dd-MM-yyyy"));
            parameters.Add("TituloReporte", "Reporte Asistencia Estudiante");
            parameters.Add("GrupoNombre", grupoNombre);
            parameters.Add("MateriaNombre", materiaNombre);

            var result = report.Execute(RenderType.Pdf, extension, parameters, mimetype);

            return File(result.MainStream, "application/pdf", "ReporteAsistencia_" + DateTime.Now.Ticks + ".pdf");
        }

        private IAsistenciaEstudianteHeaderServiceApi GetIAsistenciaEstudianteHeaderServiceApi()
        {
            return RestService.For<IAsistenciaEstudianteHeaderServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
        }

        private IEstudianteMateriaServiceApi GetIEstudianteMateriaServiceApi()
        {
            return RestService.For<IEstudianteMateriaServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
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
        private IMateriaServiceApi GetIMateriaServiceApi()
        {
            return RestService.For<IMateriaServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
        }
    }
}
