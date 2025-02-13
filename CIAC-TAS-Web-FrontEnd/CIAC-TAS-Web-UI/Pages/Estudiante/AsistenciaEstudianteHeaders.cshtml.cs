using AspNetCore.Reporting;
using CIAC_TAS_Service.Contracts.V1.Responses;
using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.Helper;
using CIAC_TAS_Web_UI.ModelViews.Estudiante;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Refit;
using System.Data;
using System.Text;
using static CIAC_TAS_Service.Contracts.V1.ApiRoute;
using static CIAC_TAS_Web_UI.Helper.EnumsGlobales;

namespace CIAC_TAS_Web_UI.Pages.Estudiante
{
    public class AsistenciaEstudianteHeadersModel : PageModel
    {
        [BindProperty]
        public List<AsistenciaEstudianteHeaderModelView> AsistenciaEstudianteHeaderModelView { get; set; } = new List<AsistenciaEstudianteHeaderModelView>();
        public string GrupoNombre { get; set; }
        public string MateriaNombre { get; set; }

        [TempData]
        public string Message { get; set; }

        private readonly IConfiguration _configuration;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment;

        public AsistenciaEstudianteHeadersModel(IConfiguration configuration, Microsoft.AspNetCore.Hosting.IHostingEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        public async Task<IActionResult> OnGetAsync(int grupoId, int materiaId)
        {
            var asistenciaEstudianteHeaderServiceApi = RestService.For<IAsistenciaEstudianteHeaderServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });

            var grupoServiceApi = RestService.For<IGrupoServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });

            var materiaServiceApi = RestService.For<IMateriaServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });

            var asistenciaEstudianteHeadersResponse = await asistenciaEstudianteHeaderServiceApi.GetAllHeadersByGrupoIdMateriaIdAsync(grupoId, materiaId);

            if (!asistenciaEstudianteHeadersResponse.IsSuccessStatusCode)
            {
                Message = "Ocurrio un error inesperado";

                return RedirectToPage("/Estudiante/AsistenciaEstudianteHeadersPreview");
            }

            var asistenciaEstudianteHeaders = asistenciaEstudianteHeadersResponse.Content.Data;
            AsistenciaEstudianteHeaderModelView = asistenciaEstudianteHeaders.Select(x => new AsistenciaEstudianteHeaderModelView
            {
                Id = x.Id,
                ProgramaNombre = x.ProgramaResponse.Nombre,
                GrupoNombre = x.GrupoResponse.Nombre,
                MateriaNombre = x.MateriaResponse.Nombre,
                ModuloNombre = x.ModuloResponse.Nombre,
                InstructorNombre = x.InstructorResponse.Nombres + " " + x.InstructorResponse.ApellidoPaterno,
                Fecha = x.Fecha
            }).ToList();

            var grupoResponse = await grupoServiceApi.GetAsync(grupoId);
            if (grupoResponse.IsSuccessStatusCode)
            {
                GrupoNombre = grupoResponse.Content.Nombre;
            }

            var materiaResponse = await materiaServiceApi.GetAsync(materiaId);
            if (materiaResponse.IsSuccessStatusCode)
            {
                MateriaNombre = materiaResponse.Content.Nombre;
            }

            return Page();
        }

        public async Task<IActionResult> OnGetDownloadAsistenciaEstudiantesReportAsync(int grupoId, int materiaId)
        {
            var asistenciaEstudianteHeaderServiceApi = RestService.For<IAsistenciaEstudianteHeaderServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });

            var grupoServiceApi = RestService.For<IGrupoServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });

            var materiaServiceApi = RestService.For<IMateriaServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });

            var estudianteMateriaServiceApi = RestService.For<IEstudianteMateriaServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });

            var asistenciaEstudianteHeadersResponse = await asistenciaEstudianteHeaderServiceApi.GetAllHeadersByGrupoIdMateriaIdAsync(grupoId, materiaId);

            if (!asistenciaEstudianteHeadersResponse.IsSuccessStatusCode)
            {
                Message = "Ocurrio un error inesperado";

                return RedirectToPage("/Estudiante/AsistenciaEstudianteHeaders", new { grupoId = grupoId, materiaId = materiaId });
            }

            var estudianteMateriaResponse = await estudianteMateriaServiceApi.GetAllByMateriaGrupoAsync(materiaId, grupoId);

            if (!estudianteMateriaResponse.IsSuccessStatusCode)
            {
                Message = "Ocurrio un error inesperado";

                return RedirectToPage("/Estudiante/AsistenciaEstudianteHeaders", new { grupoId = grupoId, materiaId = materiaId });
            }

            var estudianteMaterias = estudianteMateriaResponse.Content.Data;
            var asistenciaEstudianteHeaders = asistenciaEstudianteHeadersResponse.Content.Data;            

            var grupoResponse = await grupoServiceApi.GetAsync(grupoId);
            var grupoNombre = string.Empty;
            if (grupoResponse.IsSuccessStatusCode)
            {
                grupoNombre = grupoResponse.Content.Nombre;
            }

            var materiaResponse = await materiaServiceApi.GetAsync(materiaId);
            var materiaNombre = string.Empty;
            if (materiaResponse.IsSuccessStatusCode)
            {
                materiaNombre = materiaResponse.Content.Nombre;
            }

            string renderFormart = "PDF";
            string mimetype = "";
            int extension = 1;
            string reportPath = Path.Combine(_environment.WebRootPath, "Reports/ASA/ReporteAsistenciaEstudiantes.rdlc");
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("EstudianteId");
            dataTable.Columns.Add("EstudianteNombre");
            dataTable.Columns.Add("FechaAsistencia");
            dataTable.Columns.Add("Asistencia");
            dataTable.Columns.Add("AsistenciaPorcentajeRegular");
            dataTable.Columns.Add("AsistenciaPorcentajeTutorial");
            dataTable.Columns.Add("InscritoTutorial");
            dataTable.Columns.Add("TipoAsistenciaEstudiante");

            var asistenciaRegularTotal = asistenciaEstudianteHeaders
                .Where(x => x.TipoAsistenciaEstudianteHeaderId == (int)TipoAsistenciaEstudianteHeaderEnum.Regular)
                .Count();

            var asistenciaTutorialTotal = asistenciaEstudianteHeaders
                .Where(x => x.TipoAsistenciaEstudianteHeaderId == (int)TipoAsistenciaEstudianteHeaderEnum.Tutorial)
                .Count();

            foreach (var estudianteMateriaItem in estudianteMaterias)
            {
                var asistenciaEstudianteTotalesRegular = 0;
                var asistenciaEstudianteTotalesTutorial = 0;
                foreach (var asistenciaEstudianteDetail in asistenciaEstudianteHeaders)
                {
                    DataRow dataRow = dataTable.NewRow();
                    dataRow["EstudianteId"] = estudianteMateriaItem.EstudianteId;
                    dataRow["EstudianteNombre"] = estudianteMateriaItem.EstudianteResponse.Nombre + " " + estudianteMateriaItem.EstudianteResponse.ApellidoPaterno + " " + estudianteMateriaItem.EstudianteResponse.ApellidoMaterno;
                    dataRow["FechaAsistencia"] = asistenciaEstudianteDetail.Fecha.ToString("dd-MM-yyyy");
                    var asistencia = asistenciaEstudianteDetail.AsistenciaEstudiantesResponse
                        .Where(x => x.EstudianteId == estudianteMateriaItem.EstudianteId)
                        .Select(x => x.TipoAsistenciaResponse.Nombre)
                        .FirstOrDefault();
                    dataRow["Asistencia"] = asistencia == null ? "No reportado" : asistencia;
                    dataRow["AsistenciaPorcentajeRegular"] = 0;
                    dataRow["AsistenciaPorcentajeTutorial"] = 0;
                    dataRow["InscritoTutorial"] = estudianteMateriaItem.InscritoTutorial ? "Si" : "No";
                    dataRow["TipoAsistenciaEstudiante"] = asistenciaEstudianteDetail.TipoAsistenciaEstudianteHeaderResponse.Nombre;

                    dataTable.Rows.Add(dataRow);

                    if (asistencia != null && (asistencia == "Justificada" || asistencia == "Presente"))
                    {
                        if (estudianteMateriaItem.InscritoTutorial && //Si el estudiante esta inscrito en Tutorial y la materia es Tutorial
                            asistenciaEstudianteDetail.TipoAsistenciaEstudianteHeaderResponse.Nombre == "Tutorial"
                        )  { 
                            asistenciaEstudianteTotalesTutorial++;
                        } else if (asistenciaEstudianteDetail.TipoAsistenciaEstudianteHeaderResponse.Nombre == "Regular")
                        {
                            asistenciaEstudianteTotalesRegular++;
                        }
                        
                    }
                }

                var rows = dataTable.Select("EstudianteId=" + estudianteMateriaItem.EstudianteId);
                var asistenciaFinalRegular = asistenciaRegularTotal == 0 ? 0 : (asistenciaEstudianteTotalesRegular * 100) / asistenciaRegularTotal;
                var asistenciaFinalTutorial = asistenciaTutorialTotal == 0 ? 0 : (asistenciaEstudianteTotalesTutorial * 100) / asistenciaTutorialTotal;
                
                foreach (DataRow row in rows)
                {
                    row["AsistenciaPorcentajeRegular"] = asistenciaFinalRegular;
                    row["AsistenciaPorcentajeTutorial"] = asistenciaFinalTutorial;
                }
            }

            var report = new LocalReport(reportPath);
            report.AddDataSource("dsAsistenciaEstudiante", dataTable);

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("Fecha", DateTime.Today.ToString("dd-MM-yyyy"));
            parameters.Add("TituloReporte", "Reporte Asistencia Estudiantes");
            parameters.Add("GrupoNombre", grupoNombre);
            parameters.Add("MateriaNombre", materiaNombre);

            var result = report.Execute(RenderType.Pdf, extension, parameters, mimetype);

            return File(result.MainStream, "application/pdf", "ReporteAsistenciaEstudiantes_" + DateTime.Now.Ticks + ".pdf");
        }
    }
}
