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

namespace CIAC_TAS_Web_UI.Pages.Instructor
{
    public class RegistroNotasHeadersModel : PageModel
    {
        [BindProperty]
        public List<RegistroNotaHeadersModelView> RegistroNotaHeadersModelView { get; set; } = new List<RegistroNotaHeadersModelView>();
        public string GrupoNombre { get; set; }
        public string MateriaNombre { get; set; }

        [TempData]
        public string Message { get; set; }

        private readonly IConfiguration _configuration;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment;

        public RegistroNotasHeadersModel(IConfiguration configuration, Microsoft.AspNetCore.Hosting.IHostingEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        public async Task<IActionResult> OnGetAsync(int grupoId, int materiaId)
        {
            var registroNotaHeaderServiceApi = GetIRegistroNotaHeaderServiceApi();
            var registroNotaHeaderResponse = await registroNotaHeaderServiceApi.GetAllHeadersByGrupoAndMateriaIdAsync(grupoId, materiaId);

            if (!registroNotaHeaderResponse.IsSuccessStatusCode)
            {
                Message = "Ocurrio un error inesperado";

                return RedirectToPage("/Instructor/RegistroNotasList");
            }

            var registroNotaHeaders = registroNotaHeaderResponse.Content.Data;
            RegistroNotaHeadersModelView = registroNotaHeaders.Select(x => new RegistroNotaHeadersModelView
            {
                Id = x.Id,
                GrupoId = x.GrupoId,
                MateriaId = x.MateriaId,
                ProgramaNombre = x.Programa.Nombre,
                GrupoNombre = x.Grupo.Nombre,
                MateriaNombre = x.Materia.Nombre,
                ModuloNombre = x.Modulo.Nombre,
                InstructorNombre = x.Instructor.Nombres + " " + x.Instructor.ApellidoPaterno,
                TipoRegistroNotaHeaderNombre = x.TipoRegistroNotaHeader.Nombre,
                TipoRegistroNotaHeaderId = x.TipoRegistroNotaHeaderId
            }).ToList();

            var grupoServiceApi = GetIGrupoServiceApi();
            var grupoResponse = await grupoServiceApi.GetAsync(grupoId);
            if (grupoResponse.IsSuccessStatusCode)
            {
                GrupoNombre = grupoResponse.Content.Nombre;
            }

            var materiaServiceApi = GetIMateriaServiceApi();
            var materiaResponse = await materiaServiceApi.GetAsync(materiaId);
            if (materiaResponse.IsSuccessStatusCode)
            {
                MateriaNombre = materiaResponse.Content.Nombre;
            }

            return Page();
        }


        public async Task<IActionResult> OnGetDownloadRegistroNotaEstudianteHeadersAsync(int registroNotaHeaderId, int grupoId, int materiaId)
        {
            var registroNotaHeaderServiceApi = GetIRegistroNotaHeaderServiceApi();
            var registroNotaHeaderResponse = await registroNotaHeaderServiceApi.GetAsync(registroNotaHeaderId);

            if (!registroNotaHeaderResponse.IsSuccessStatusCode)
            {
                Message = "Ocurrio un error inesperado";

                return RedirectToPage("/Instructor/RegistroNotasHeaders", new { grupoId = grupoId, materiaId = materiaId });
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
            string reportPath = Path.Combine(_environment.WebRootPath, "Reports/Estudiante/ReporteRegistroNotasEstudiantes.rdlc");
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("EstudianteId");
            dataTable.Columns.Add("EstudianteNombre");
            dataTable.Columns.Add("Nota");
            dataTable.Columns.Add("PorcentajeDominio");
            dataTable.Columns.Add("PorcentajeProgreso");
            dataTable.Columns.Add("NotaFinal"); //falta este
            dataTable.Columns.Add("EsTutorial");
            dataTable.Columns.Add("NotaProgresoTotal");
            dataTable.Columns.Add("NotaDominioTotal");
            dataTable.Columns.Add("NotaRecuperatorioTotal");
            dataTable.Columns.Add("ExisteRecuperatorio");
            dataTable.Columns.Add("TipoRegistroNotaEstudiante");

            var registroNotaHeaders = registroNotaHeaderResponse.Content;
            var instructorNombre = registroNotaHeaders.Instructor.Nombres + " " + registroNotaHeaders.Instructor.ApellidoPaterno;
            var porcentajeDominio = registroNotaHeaders.PorcentajeDominioTotal;
            var porcentajeProgreso = registroNotaHeaders.PorcentajeProgresoTotal;
            var esTutorial = registroNotaHeaders.TipoRegistroNotaHeaderId == (int)TipoRegistroNotaHeaderEnum.Tutorial;

            foreach (var registroNotaEstudianteHeader in registroNotaHeaders.RegistroNotaEstudianteHeaders)
            {
                var nombreEstudiante = registroNotaEstudianteHeader.Estudiante.Nombre + " " + registroNotaEstudianteHeader.Estudiante.ApellidoPaterno + " " + registroNotaEstudianteHeader.Estudiante.ApellidoMaterno;
                var estudianteId = registroNotaEstudianteHeader.Estudiante.Id;
                var notasProgreso = new List<double>();
                var notasDominio = new List<double>();
                var notasRecuperatorio = new List<double>();

                foreach (var registroNotaEstudiante in registroNotaEstudianteHeader.RegistroNotaEstudiantes)
                {
                    var nota = registroNotaEstudiante.Nota;
                    DataRow dataRow = dataTable.NewRow();
                    dataRow["EstudianteId"] = estudianteId;
                    dataRow["EstudianteNombre"] = nombreEstudiante;
                    dataRow["EsTutorial"] = esTutorial ? "Si" : "No";
                    dataRow["PorcentajeDominio"] = porcentajeDominio;
                    dataRow["PorcentajeProgreso"] = porcentajeProgreso;
                    dataRow["Nota"] = nota;
                    

                    dataTable.Rows.Add(dataRow);

                    if (registroNotaEstudiante.TipoRegistroNotaEstudianteId == (int)TipoRegistroNotaEstudianteEnum.Progreso)
                    {
                        notasProgreso.Add(nota);
                        dataRow["TipoRegistroNotaEstudiante"] = "Progreso";
                    } 
                    else if (registroNotaEstudiante.TipoRegistroNotaEstudianteId == (int)TipoRegistroNotaEstudianteEnum.Dominio)
                    {
                        notasDominio.Add(nota);
                        dataRow["TipoRegistroNotaEstudiante"] = "Dominio";
                    }
                    else if (registroNotaEstudiante.TipoRegistroNotaEstudianteId == (int)TipoRegistroNotaEstudianteEnum.Recuperatorio)
                    {
                        notasRecuperatorio.Add(nota);
                        dataRow["TipoRegistroNotaEstudiante"] = "Recuperatorio";
                    }
                }
                
                var rows = dataTable.Select("EstudianteId=" + estudianteId);
                var notaProgresoTotal = notasProgreso.Count() > 0 ? (notasProgreso.Average() * (porcentajeProgreso/100.0)) : 0;
                var notaDominioTotal = notasDominio.Count() > 0 ? notasDominio.Average() * (porcentajeDominio / 100.0) : 0;
                var notaRecuperatorioTotal = notasRecuperatorio.Count > 0 ? notasRecuperatorio.Average() * 0.75 : 0;

                foreach (DataRow row in rows)
                {
                    row["NotaProgresoTotal"] = notaProgresoTotal;
                    row["NotaDominioTotal"] = notaDominioTotal;
                    row["NotaRecuperatorioTotal"] = notaRecuperatorioTotal;
                    row["ExisteRecuperatorio"] = notasRecuperatorio.Count() > 0 ? "Si" : "No";
                }
            }

            var report = new LocalReport(reportPath);
            report.AddDataSource("dsRegistroNotasEstudiantes", dataTable);

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("Fecha", DateTime.Today.ToString("dd-MM-yyyy"));
            parameters.Add("TituloReporte", "Reporte Registro Notas");
            parameters.Add("GrupoNombre", grupoNombre);
            parameters.Add("MateriaNombre", materiaNombre);
            parameters.Add("InstructorNombre", instructorNombre);

            var result = report.Execute(RenderType.Pdf, extension, parameters, mimetype);

            return File(result.MainStream, "application/pdf", "ReporteRegistroNotas_" + DateTime.Now.Ticks + ".pdf");
        }

        private IRegistroNotaHeaderServiceApi GetIRegistroNotaHeaderServiceApi()
        {
            return RestService.For<IRegistroNotaHeaderServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
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
