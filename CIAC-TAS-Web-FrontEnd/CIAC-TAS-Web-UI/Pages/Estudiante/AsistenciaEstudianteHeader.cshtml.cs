using CIAC_TAS_Service.Contracts.V1.Requests;
using CIAC_TAS_Service.Contracts.V1.Responses;
using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.Helper;
using CIAC_TAS_Web_UI.ModelViews.Estudiante;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Refit;

namespace CIAC_TAS_Web_UI.Pages.Estudiante
{
    public class AsistenciaEstudianteHeaderModel : PageModel
    {
        [BindProperty]
        public AsistenciaEstudianteHeaderModelView AsistenciaEstudianteHeaderModelView { get; set; }

        //[BindProperty]
        //public List<EstudianteModelView> EstudiantesModelView { get; set; }

        public List<SelectListItem> ProgramaOptions { get; set; }
        public List<SelectListItem> GrupoOptions { get; set; }
        public List<SelectListItem> MateriaOptions { get; set; }
        public List<SelectListItem> ModuloOptions { get; set; }
        public List<SelectListItem> InstructorOptions { get; set; }
        public List<SelectListItem> TipoAsistenciaOptions { get; set; }
        
        public int GrupoId { get; set; }
        public int MateriaId { get; set; }
        public string GrupoNombre { get; set; }
        public string MateriaNombre { get; set; }

        [TempData]
        public string Message { get; set; }

        private readonly IConfiguration _configuration;
        public AsistenciaEstudianteHeaderModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> OnGetNewAsistenciaEstudianteHeaderAsync(int grupoId, int materiaId)
        {
            await SetExtraData(grupoId, materiaId);

            await FillSelectListsItems();

            return Page();
        }               

        public async Task<IActionResult> OnPostNewAsistenciaEstudianteHeaderAsync(int grupoId, int materiaId)
        {
            if (!ModelState.IsValid)
            {
                await SetExtraData(grupoId, materiaId);

                Message = "Por favor complete el formulario correctamente";

                await FillSelectListsItems();

                return Page();
            }
                        
            // ProgramaId 1 es TMA
            AsistenciaEstudianteHeaderModelView.ProgramaId = 1;
            AsistenciaEstudianteHeaderModelView.GrupoId = grupoId;
            AsistenciaEstudianteHeaderModelView.MateriaId = materiaId;

            var moduloMateriaServiceApi = GetIModuloMateriaServiceApi();
            var moduloMateriaServiceResponse = await moduloMateriaServiceApi.GetModuloByMateriaAsync(materiaId);
            if (moduloMateriaServiceResponse.IsSuccessStatusCode)
            {
                AsistenciaEstudianteHeaderModelView.ModuloId = moduloMateriaServiceResponse.Content.ModuloId;
            }

            var asistenciaEstudianteHeaderApi = GetIAsistenciaEstudianteHeaderServiceApi();
            var createAsistenciaEstudianteHeaderRequest = new CreateAsistenciaEstudianteHeaderRequest
            {
                ProgramaId = AsistenciaEstudianteHeaderModelView.ProgramaId,
                GrupoId = AsistenciaEstudianteHeaderModelView.GrupoId,
                MateriaId = AsistenciaEstudianteHeaderModelView.MateriaId,
                ModuloId = AsistenciaEstudianteHeaderModelView.ModuloId,
                InstructorId = AsistenciaEstudianteHeaderModelView.InstructorId,
                Fecha = AsistenciaEstudianteHeaderModelView.Fecha,
                HoraInicio = AsistenciaEstudianteHeaderModelView.HoraInicio,
                HoraFin = AsistenciaEstudianteHeaderModelView.HoraFin,
                TotalHorasTeoricas = AsistenciaEstudianteHeaderModelView.TotalHorasTeoricas,
                TotalHorasPracticas = AsistenciaEstudianteHeaderModelView.TotalHorasPracticas,
                Tema = AsistenciaEstudianteHeaderModelView.Tema
            };

            var createAsistenciaEstudianteHeaderResponse = await asistenciaEstudianteHeaderApi.CreateAsync(createAsistenciaEstudianteHeaderRequest);
            if (!createAsistenciaEstudianteHeaderResponse.IsSuccessStatusCode)
            {
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(createAsistenciaEstudianteHeaderResponse.Error.Content);

                Message = String.Join(" ", errorResponse.Errors.Select(x => x.Message));

                await SetExtraData(grupoId, materiaId);
                await FillSelectListsItems();

                return Page();
            }

            var estudianteMateriaServiceApi = GetIEstudianteMateriaServiceApi();
            var estudianteMateriaResponse = await estudianteMateriaServiceApi.GetAllByMateriaGrupoAsync(materiaId, grupoId);

            if (estudianteMateriaResponse.IsSuccessStatusCode)
            {
                var createAsistenciaEstudianteRequest = estudianteMateriaResponse.Content.Data
                .Select(x => new CreateAsistenciaEstudianteRequest
                {
                    EstudianteId = x.EstudianteId,
                    AsistenciaEstudianteHeaderId = createAsistenciaEstudianteHeaderResponse.Content.Id,
                    TipoAsistenciaId = 1 //Tipo Asistencia Presente
                }).ToList();

                var asistenciaEstudianteServiceApi = GetIAsistenciaEstudianteServiceApi();
                var createBatchResponse = await asistenciaEstudianteServiceApi.CreateBatchAsync(createAsistenciaEstudianteRequest);             
            }            

            return RedirectToPage("/Estudiante/AsistenciaEstudianteHeader", "EditAsistenciaEstudianteHeader", new { id = createAsistenciaEstudianteHeaderResponse.Content.Id, grupoId = grupoId, materiaId = materiaId });
        }

        public async Task<IActionResult> OnGetEditAsistenciaEstudianteHeaderAsync(int id, int grupoId, int materiaId)
        {
            await SetExtraData(grupoId, materiaId);

            var asistenciaEstudianteHeaderApi = GetIAsistenciaEstudianteHeaderServiceApi();
            var asistenciaEstudianteHeaderResponse = await asistenciaEstudianteHeaderApi.GetAsync(id);

            if (!asistenciaEstudianteHeaderResponse.IsSuccessStatusCode)
            {
                Message = "Ocurrio un error inesperado";

                return RedirectToPage("/Estudiante/AsistenciaEstudianteHeaders", new { grupoId = grupoId, materiaId = materiaId });
            }

            await FillSelectListsItems();

            var asistenciaEstudianteHeader = asistenciaEstudianteHeaderResponse.Content;
            AsistenciaEstudianteHeaderModelView = new AsistenciaEstudianteHeaderModelView
            {
                Id = asistenciaEstudianteHeader.Id,
                ProgramaId = asistenciaEstudianteHeader.ProgramaId,
                GrupoId = asistenciaEstudianteHeader.GrupoId,
                MateriaId = asistenciaEstudianteHeader.MateriaId,
                ModuloId = asistenciaEstudianteHeader.ModuloId,
                InstructorId = asistenciaEstudianteHeader.InstructorId,
                Fecha = asistenciaEstudianteHeader.Fecha,
                HoraInicio = asistenciaEstudianteHeader.HoraInicio,
                HoraFin = asistenciaEstudianteHeader.HoraFin,
                TotalHorasPracticas = asistenciaEstudianteHeader.TotalHorasPracticas,
                TotalHorasTeoricas = asistenciaEstudianteHeader.TotalHorasTeoricas,
                Tema = asistenciaEstudianteHeader.Tema,
                AsistenciaEstudianteModelView = asistenciaEstudianteHeader.AsistenciaEstudiantesResponse.Select(
                    x => new AsistenciaEstudianteModelView
                    {
                        Id = x.Id,
                        EstudianteId = x.EstudianteId,
                        EstudianteNombre = x.EstudianteResponse.Nombre + " " + x.EstudianteResponse.ApellidoPaterno,
                        TipoAsistenciaId = x.TipoAsistenciaId,
                        TipoAsistenciaNombre = x.TipoAsistenciaResponse.Nombre
                    }).ToList(),
            };

            return Page();
        }

        public async Task<IActionResult> OnPostEditAsistenciaEstudianteHeaderAsync(int id, int grupoId, int materiaId)
        {
            if (!ModelState.IsValid)
            {
                Message = "Por favor complete el formulario correctamente";

                await FillSelectListsItems();

                return Page();
            }            

            var asistenciaEstudianteHeaderServiceApi = GetIAsistenciaEstudianteHeaderServiceApi();
            var asistenciaEstudianteHeaderRequest = new UpdateAsistenciaEstudianteHeaderRequest
            {
                ProgramaId = AsistenciaEstudianteHeaderModelView.ProgramaId,
                GrupoId = AsistenciaEstudianteHeaderModelView.GrupoId,
                MateriaId = AsistenciaEstudianteHeaderModelView.MateriaId,
                ModuloId = AsistenciaEstudianteHeaderModelView.ModuloId,
                InstructorId = AsistenciaEstudianteHeaderModelView.InstructorId,
                Fecha = AsistenciaEstudianteHeaderModelView.Fecha,
                HoraInicio = AsistenciaEstudianteHeaderModelView.HoraInicio,
                HoraFin = AsistenciaEstudianteHeaderModelView.HoraFin,
                TotalHorasTeoricas = AsistenciaEstudianteHeaderModelView.TotalHorasTeoricas,
                TotalHorasPracticas = AsistenciaEstudianteHeaderModelView.TotalHorasPracticas,
                Tema = AsistenciaEstudianteHeaderModelView.Tema
            };

            var asistenciaEstudianteHeaderResponse = await asistenciaEstudianteHeaderServiceApi.UpdateAsync(id, asistenciaEstudianteHeaderRequest);
            if (!asistenciaEstudianteHeaderResponse.IsSuccessStatusCode)
            {
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(asistenciaEstudianteHeaderResponse.Error.Content);

                Message = String.Join(" ", errorResponse.Errors.Select(x => x.Message));

                await FillSelectListsItems();

                return Page();
            }

            return RedirectToPage("/Estudiante/AsistenciaEstudianteHeaders", new { grupoId = grupoId, materiaId = materiaId });
        }

        public async Task<IActionResult> OnGetRemoveAsistenciaEstudianteAsync(int asistenciaEstudianteHeaderId, int asistenciaEstudianteId, int grupoId, int materiaId)
        {
            var asistenciaEstudianteServiceApi = GetIAsistenciaEstudianteServiceApi();
            var asistenciaEstudianteResponse = await asistenciaEstudianteServiceApi.DeleteAsync(asistenciaEstudianteId);

            if (!asistenciaEstudianteResponse.IsSuccessStatusCode)
            {
                Message = "Ocurrio un error inesperado";

                return RedirectToPage("/Estudiante/AsistenciaEstudianteHeader", "EditAsistenciaEstudianteHeader", new { id = asistenciaEstudianteHeaderId, grupoId = grupoId, materiaId = materiaId });
            }

            return RedirectToPage("/Estudiante/AsistenciaEstudianteHeader", "EditAsistenciaEstudianteHeader", new { id = asistenciaEstudianteHeaderId, grupoId = grupoId, materiaId = materiaId });
        }

        public async Task<JsonResult> OnGetPatchTipoAsistenciaAsync(int asistenciaEstudianteId, int tipoAsistenciaId)
        {
            if (asistenciaEstudianteId == 0 || tipoAsistenciaId == 0)
            {
                return new JsonResult("Error al intentar actualizar la asistencia, valores no validos");
            }

            var asistenciaEstudianteServiceApi = GetIAsistenciaEstudianteServiceApi();
            var asistenciaEstudianteServiceResponse = await asistenciaEstudianteServiceApi.PatchTipoAsistenciaIdAsync(asistenciaEstudianteId, new PatchAsistenciaEstudianteRequest
            {
                TipoAsistenciaId = tipoAsistenciaId
            });

            if (!asistenciaEstudianteServiceResponse.IsSuccessStatusCode)
            {
                return new JsonResult("Error al intentar actualizar la asistencia");
            }

            return new JsonResult("Actualizacion correcta");
        }


        private IProgramaServiceApi GetIProgramaServiceApi()
        {
            return RestService.For<IProgramaServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
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

        private IModuloServiceApi GetIModuloServiceApi()
        {
            return RestService.For<IModuloServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
        }

        private IInstructorServiceApi GetIInstructorServiceApi()
        {
            return RestService.For<IInstructorServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
        }

        private IAsistenciaEstudianteHeaderServiceApi GetIAsistenciaEstudianteHeaderServiceApi()
        {
            return RestService.For<IAsistenciaEstudianteHeaderServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
        }

        private IAsistenciaEstudianteServiceApi GetIAsistenciaEstudianteServiceApi()
        {
            return RestService.For<IAsistenciaEstudianteServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
        }

        private IModuloMateriaServiceApi GetIModuloMateriaServiceApi()
        {
            return RestService.For<IModuloMateriaServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
        }

        private IEstudianteGrupoServiceApi GetIEstudianteGrupoServiceApi()
        {
            return RestService.For<IEstudianteGrupoServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
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

        private ITipoAsistenciaServiceApi GetITipoAsistenciaServiceApi()
        {
            return RestService.For<ITipoAsistenciaServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
        }

        private async Task FillSelectListsItems()
        {
            //var programaServiceApi = GetIProgramaServiceApi();
            //var programaResponse = await programaServiceApi.GetAllAsync();

            //if (programaResponse.IsSuccessStatusCode)
            //{
            //    ProgramaOptions = programaResponse.Content.Data.Select(x => new SelectListItem { Text = x.Nombre, Value = x.Id.ToString() }).ToList();
            //}

            //var grupoServiceApi = GetIGrupoServiceApi();
            //var grupoResponse = await grupoServiceApi.GetAllAsync();

            //if (grupoResponse.IsSuccessStatusCode)
            //{
            //    GrupoOptions = grupoResponse.Content.Data.Select(x => new SelectListItem { Text = x.Nombre, Value = x.Id.ToString() }).ToList();
            //}

            //var materiaServiceApi = GetIMateriaServiceApi();
            //var materiaResponse = await materiaServiceApi.GetAllAsync();

            //if (materiaResponse.IsSuccessStatusCode)
            //{
            //    MateriaOptions = materiaResponse.Content.Data.Select(x => new SelectListItem { Text = x.Nombre, Value = x.Id.ToString() }).ToList();
            //}

            //var moduloServiceApi = GetIModuloServiceApi();
            //var moduloResponse = await moduloServiceApi.GetAllAsync();

            //if (moduloResponse.IsSuccessStatusCode)
            //{
            //    ModuloOptions = moduloResponse.Content.Data.Select(x => new SelectListItem { Text = x.Nombre, Value = x.Id.ToString() }).ToList();
            //}

            var instructorServiceApi = GetIInstructorServiceApi();
            var instructorResponse = await instructorServiceApi.GetAllAsync();

            if (instructorResponse.IsSuccessStatusCode)
            {
                InstructorOptions = instructorResponse.Content.Data.Select(x => new SelectListItem { Text = x.Nombres + " " + x.ApellidoPaterno, Value = x.Id.ToString() }).ToList();
            }

            var tipoAsistenciaServiceApi = GetITipoAsistenciaServiceApi();
            var tipoAsistenciaResponse = await tipoAsistenciaServiceApi.GetAllAsync();

            if (tipoAsistenciaResponse.IsSuccessStatusCode)
            {
                TipoAsistenciaOptions = tipoAsistenciaResponse.Content.Data.Select(x => new SelectListItem { Text = x.Nombre, Value = x.Id.ToString() }).ToList();
            }
        }

        private async Task SetExtraData(int grupoId, int materiaId)
        {
            GrupoId = grupoId;
            MateriaId = materiaId;

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
        }
    }
}
