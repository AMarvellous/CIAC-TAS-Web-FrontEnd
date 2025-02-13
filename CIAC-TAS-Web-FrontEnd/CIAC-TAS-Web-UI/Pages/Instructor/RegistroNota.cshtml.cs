using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.Helper;
using CIAC_TAS_Web_UI.ModelViews.Instructores;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Refit;
using static CIAC_TAS_Service.Contracts.V1.ApiRoute;
using static CIAC_TAS_Web_UI.Helper.EnumsGlobales;

namespace CIAC_TAS_Web_UI.Pages.Instructor
{
    public class RegistroNotaModel : PageModel
    {
        [BindProperty]
        public List<RegistroNotaEstudianteModelView> RegistroNotaEstudianteModelView { get; set; } = new List<RegistroNotaEstudianteModelView> ();
        public int GrupoId { get; set; }
        public int MateriaId { get; set; }
        public int EstudianteId { get; set; }
        public string GrupoNombre { get; set; }
        public string MateriaNombre { get; set; }
        public string EstudianteNombre { get; set; }
        public int RegistroNotaEstudianteHeaderId { get; set; }
        public int RegistroNotaHeaderId { get; set; }
        public double NotaEstudiante { get; set; }
        public List<SelectListItem> NotaEstudianteOptions { get; set; }
        public int TipoRegistroNotaEstudianteIdSelected { get; set; }
        [TempData]
        public string Message { get; set; }
        private readonly IConfiguration _configuration;

        public RegistroNotaModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task OnGetViewRegistroNotaEstudianteAsync(int registroNotaEstudianteHeaderId, int grupoId, int materiaId, int registroNotaHeaderId, int estudianteId)
        {
            RegistroNotaEstudianteHeaderId = registroNotaEstudianteHeaderId;
            RegistroNotaHeaderId = registroNotaHeaderId;
            NotaEstudianteOptions = Enum.GetValues(typeof(TipoRegistroNotaEstudianteEnum)).Cast<TipoRegistroNotaEstudianteEnum>()
                .Select(x => new SelectListItem { Text = x.ToString(), Value = ((int)x).ToString()})
                .ToList();
            await SetExtraData(grupoId, materiaId, estudianteId);

            var registroNotaEstudianteServiceApi = GetIRegistroNotaEstudianteServiceApi();
            var registroNotaEstudianteResponse = await registroNotaEstudianteServiceApi.GetAllByRegistroNotaEstudianteHeaderIdAsync(registroNotaEstudianteHeaderId);

            if (registroNotaEstudianteResponse.IsSuccessStatusCode)
            {
                var registroNotaEstudiantes = registroNotaEstudianteResponse.Content.Data;
                RegistroNotaEstudianteModelView = registroNotaEstudiantes.Select(x => new ModelViews.Instructores.RegistroNotaEstudianteModelView
                {
                    Id = x.Id,
                    RegistroNotaEstudianteHeaderId = x.RegistroNotaEstudianteHeaderId,
                    Nota = x.Nota,
                    TipoRegistroNotaEstudianteId = x.TipoRegistroNotaEstudianteId,
                    TipoRegistroNotaEstudianteNombre = x.TipoRegistroNotaEstudiante.Nombre,
                }).ToList();
            }
        }

        public async Task<JsonResult> OnGetAddRegistroNotaEstudianteAsync(int registroNotaEstudianteHeaderId, int tipoRegistroNotaEstudianteId, double notaEstudiante)
        {
            var registroNotaEstudianteServiceApi = GetIRegistroNotaEstudianteServiceApi();
            var respuestasAsaServiceResponse = await registroNotaEstudianteServiceApi.CreateAsync(new CIAC_TAS_Service.Contracts.V1.Requests.CreateRegistroNotaEstudianteRequest
            {
                RegistroNotaEstudianteHeaderId = registroNotaEstudianteHeaderId,
                Nota = notaEstudiante,
                TipoRegistroNotaEstudianteId = tipoRegistroNotaEstudianteId
            });

            if (!respuestasAsaServiceResponse.IsSuccessStatusCode)
            {
                return new JsonResult("Error al intentar crear un registro de Nota del Estudiante");
            }

            return new JsonResult("Creacion correcta");
        }

        public async Task<JsonResult> OnGetEditRegistroNotaEstudianteAsync(int registroNotaEstudianteId, int registroNotaEstudianteHeaderId, double notaEstudiante, int tipoRegistroNotaEstudianteId)
        {
            var registroNotaEstudianteServiceApi = GetIRegistroNotaEstudianteServiceApi();
            var respuestasAsaServiceResponse = await registroNotaEstudianteServiceApi.UpdateAsync(registroNotaEstudianteId, new CIAC_TAS_Service.Contracts.V1.Requests.UpdateRegistroNotaEstudianteRequest
            {
                RegistroNotaEstudianteHeaderId = registroNotaEstudianteHeaderId,
                Nota = notaEstudiante,
                TipoRegistroNotaEstudianteId = tipoRegistroNotaEstudianteId
            });

            if (!respuestasAsaServiceResponse.IsSuccessStatusCode)
            {
                return new JsonResult("Error al intentar editar un registro de Nota del Estudiante");
            }

            return new JsonResult("Creacion correcta");
        }

        public async Task<IActionResult> OnGetDeleteRegistroNotaEstudianteAsync(int registroNotaEstudianteId, int registroNotaEstudianteHeaderId, int grupoId, int materiaId, int registroNotaHeaderId, int estudianteId)
        {
            if (registroNotaEstudianteId == 0)
            {
                Message = "Ocurrio un error al borrar el registro";

                return RedirectToPage("/Instructor/RegistroNota", "ViewRegistroNotaEstudiante", new { registroNotaEstudianteHeaderId = registroNotaEstudianteHeaderId, grupoId = grupoId, materiaId = materiaId, registroNotaHeaderId = registroNotaHeaderId, estudianteId = estudianteId });
            }

            var registroNotaEstudianteServiceApi = GetIRegistroNotaEstudianteServiceApi();
            var registroNotaEstudianteResponse = await registroNotaEstudianteServiceApi.DeleteAsync(registroNotaEstudianteId);

            if (!registroNotaEstudianteResponse.IsSuccessStatusCode)
            {
                Message = "Ocurrio un error al borrar el registro";

                return RedirectToPage("/Instructor/RegistroNota", "ViewRegistroNotaEstudiante", new { registroNotaEstudianteHeaderId = registroNotaEstudianteHeaderId, grupoId = grupoId, materiaId = materiaId, registroNotaHeaderId = registroNotaHeaderId, estudianteId = estudianteId });
            }

            return RedirectToPage("/Instructor/RegistroNota", "ViewRegistroNotaEstudiante", new { registroNotaEstudianteHeaderId = registroNotaEstudianteHeaderId, grupoId = grupoId, materiaId = materiaId, registroNotaHeaderId = registroNotaHeaderId, estudianteId = estudianteId });
        }

        private async Task SetExtraData(int grupoId, int materiaId, int estudianteId)
        {
            GrupoId = grupoId;
            MateriaId = materiaId;
            EstudianteId = estudianteId;

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

            var estudianteServiceApi = GetIEstudianteServiceApi();
            var estudianteResponse = await estudianteServiceApi.GetAsync(estudianteId);
            if (estudianteResponse.IsSuccessStatusCode)
            {
                EstudianteNombre = estudianteResponse.Content.Nombre + " " + estudianteResponse.Content.ApellidoPaterno;
            }
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

        private IRegistroNotaEstudianteServiceApi GetIRegistroNotaEstudianteServiceApi()
        {
            return RestService.For<IRegistroNotaEstudianteServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
        }

        private IEstudianteServiceApi GetIEstudianteServiceApi()
        {
            return RestService.For<IEstudianteServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
        }
    }
}
