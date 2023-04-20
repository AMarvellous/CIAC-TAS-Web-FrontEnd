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

        public List<SelectListItem> ProgramaOptions { get; set; }
        public List<SelectListItem> GrupoOptions { get; set; }
        public List<SelectListItem> MateriaOptions { get; set; }
        public List<SelectListItem> ModuloOptions { get; set; }
        public List<SelectListItem> InstructorOptions { get; set; }

        [TempData]
        public string Message { get; set; }

        private readonly IConfiguration _configuration;
        public AsistenciaEstudianteHeaderModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task OnGetNewAsistenciaEstudianteHeaderAsync()
        {
            await FillSelectListsItems();
        }

        public async Task<IActionResult> OnPostNewAsistenciaEstudianteHeaderAsync()
        {
            if (!ModelState.IsValid)
            {
                Message = "Por favor complete el formulario correctamente";

                await FillSelectListsItems();

                return Page();
            }

            var asistenciaEstudianteHeaderApi = GetIAsistenciaEstudianteHeaderServiceApi();
            var createAsistenciaEstudianteHeaderRequest = new CreateAsistenciaEstudianteHeaderRequest
            {
                ProgramaId = AsistenciaEstudianteHeaderModelView.ProgramaId,
                GrupoId = AsistenciaEstudianteHeaderModelView.GrupoId,
                MateriaId = AsistenciaEstudianteHeaderModelView.MateriaId,
                ModuloId = AsistenciaEstudianteHeaderModelView.ModuloId,
                InstructorId = AsistenciaEstudianteHeaderModelView.InstructorId,
                Fecha = AsistenciaEstudianteHeaderModelView.Fecha
            };

            var createAsistenciaEstudianteHeaderResponse = await asistenciaEstudianteHeaderApi.CreateAsync(createAsistenciaEstudianteHeaderRequest);
            if (!createAsistenciaEstudianteHeaderResponse.IsSuccessStatusCode)
            {
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(createAsistenciaEstudianteHeaderResponse.Error.Content);

                Message = String.Join(" ", errorResponse.Errors.Select(x => x.Message));

                await FillSelectListsItems();

                return Page();
            }

            return RedirectToPage("/Estudiante/AsistenciaEstudianteHeaders");
        }

        public async Task<IActionResult> OnGetEditAsistenciaEstudianteHeaderAsync(int id)
        {
            var asistenciaEstudianteHeaderApi = GetIAsistenciaEstudianteHeaderServiceApi();
            var asistenciaEstudianteHeaderResponse = await asistenciaEstudianteHeaderApi.GetAsync(id);

            if (!asistenciaEstudianteHeaderResponse.IsSuccessStatusCode)
            {
                Message = "Ocurrio un error inesperado";

                return RedirectToPage("/Estudiante/AsistenciaEstudianteHeaders");
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
                AsistenciaEstudianteModelView = asistenciaEstudianteHeader.AsistenciaEstudiantesResponse.Select(
                    x => new AsistenciaEstudianteModelView
                    {
                        Id = x.Id,
                        EstudianteId = x.EstudianteId,
                        EstudianteNombre = x.EstudianteResponse.Nombre + " " + x.EstudianteResponse.ApellidoPaterno,
                    }).ToList(),
            };

            return Page();
        }

        public async Task<IActionResult> OnPostEditAsistenciaEstudianteHeaderAsync(int id)
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
            };

            var asistenciaEstudianteHeaderResponse = await asistenciaEstudianteHeaderServiceApi.UpdateAsync(id, asistenciaEstudianteHeaderRequest);
            if (!asistenciaEstudianteHeaderResponse.IsSuccessStatusCode)
            {
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(asistenciaEstudianteHeaderResponse.Error.Content);

                Message = String.Join(" ", errorResponse.Errors.Select(x => x.Message));

                await FillSelectListsItems();

                return Page();
            }

            return RedirectToPage("/Estudiante/AsistenciaEstudianteHeaders");
        }

        public async Task<IActionResult> OnGetRemoveAsistenciaEstudianteAsync(int asistenciaEstudianteHeaderId, int asistenciaEstudianteId)
        {
            var asistenciaEstudianteServiceApi = GetIAsistenciaEstudianteServiceApi();
            var asistenciaEstudianteResponse = await asistenciaEstudianteServiceApi.DeleteAsync(asistenciaEstudianteId);

            if (!asistenciaEstudianteResponse.IsSuccessStatusCode)
            {
                Message = "Ocurrio un error inesperado";

                return RedirectToPage("/Estudiante/AsistenciaEstudianteHeader", "EditAsistenciaEstudianteHeader", new { id = asistenciaEstudianteHeaderId });
            }

            return RedirectToPage("/Estudiante/AsistenciaEstudianteHeader", "EditAsistenciaEstudianteHeader", new { id = asistenciaEstudianteHeaderId });
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

        private async Task FillSelectListsItems()
        {
            var programaServiceApi = GetIProgramaServiceApi();
            var programaResponse = await programaServiceApi.GetAllAsync();

            if (programaResponse.IsSuccessStatusCode)
            {
                ProgramaOptions = programaResponse.Content.Data.Select(x => new SelectListItem { Text = x.Nombre, Value = x.Id.ToString() }).ToList();
            }

            var grupoServiceApi = GetIGrupoServiceApi();
            var grupoResponse = await grupoServiceApi.GetAllAsync();

            if (grupoResponse.IsSuccessStatusCode)
            {
                GrupoOptions = grupoResponse.Content.Data.Select(x => new SelectListItem { Text = x.Nombre, Value = x.Id.ToString() }).ToList();
            }

            var materiaServiceApi = GetIMateriaServiceApi();
            var materiaResponse = await materiaServiceApi.GetAllAsync();

            if (materiaResponse.IsSuccessStatusCode)
            {
                MateriaOptions = materiaResponse.Content.Data.Select(x => new SelectListItem { Text = x.Nombre, Value = x.Id.ToString() }).ToList();
            }

            var moduloServiceApi = GetIModuloServiceApi();
            var moduloResponse = await moduloServiceApi.GetAllAsync();

            if (moduloResponse.IsSuccessStatusCode)
            {
                ModuloOptions = moduloResponse.Content.Data.Select(x => new SelectListItem { Text = x.Nombre, Value = x.Id.ToString() }).ToList();
            }

            var instructorServiceApi = GetIInstructorServiceApi();
            var instructorResponse = await instructorServiceApi.GetAllAsync();

            if (instructorResponse.IsSuccessStatusCode)
            {
                InstructorOptions = instructorResponse.Content.Data.Select(x => new SelectListItem { Text = x.Nombres + " " + x.ApellidoPaterno, Value = x.Id.ToString() }).ToList();
            }
        }
    }
}
