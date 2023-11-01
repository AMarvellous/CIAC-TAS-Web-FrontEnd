using CIAC_TAS_Service.Contracts.V1.Responses;
using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.Helper;
using CIAC_TAS_Web_UI.ModelViews.Estudiante;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Refit;
using static CIAC_TAS_Service.Contracts.V1.ApiRoute;

namespace CIAC_TAS_Web_UI.Pages.Instructor
{
    public class RegistroNotaEstudianteHeadersModel : PageModel
    {
        [BindProperty]
        public List<RegistroNotaEstudianteHeadersModelView> RegistroNotaEstudianteHeadersModelView { get; set; } = new List<RegistroNotaEstudianteHeadersModelView>();
        [BindProperty]
        public RegistroNotaHeadersModelView RegistroNotaHeadersModelView { get; set; } = new RegistroNotaHeadersModelView();
        public int GrupoId { get; set; }
        public int MateriaId { get; set; }
        public string GrupoNombre { get; set; }
        public string MateriaNombre { get; set; }
        public int RegistroNotaHeaderId { get; set; }
        public List<SelectListItem> InstructorOptions { get; set; }
        [TempData]
        public string Message { get; set; }

        private readonly IConfiguration _configuration;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment;

        public RegistroNotaEstudianteHeadersModel(IConfiguration configuration, Microsoft.AspNetCore.Hosting.IHostingEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        public async Task OnGetViewRegistroNotaEstudianteHeadersAsync(int registroNotaHeaderId, int grupoId, int materiaId)
        {
            RegistroNotaHeaderId = registroNotaHeaderId;
            await SetExtraData(grupoId, materiaId);
            await FillSelectListsItems();
            await SetRegistroNotaEstudianteHeaders(registroNotaHeaderId);

            var registroNotaHeaderServiceApi = GetIRegistroNotaHeaderServiceApi();
            var registroNotaHeaderResponse = await registroNotaHeaderServiceApi.GetAsync(registroNotaHeaderId);

            if (registroNotaHeaderResponse.IsSuccessStatusCode)
            {
                var registroNotaHeader = registroNotaHeaderResponse.Content;
                RegistroNotaHeadersModelView.Id = registroNotaHeader.Id;
                RegistroNotaHeadersModelView.ProgramaId = registroNotaHeader.ProgramaId;
                RegistroNotaHeadersModelView.GrupoId = registroNotaHeader.GrupoId;
                RegistroNotaHeadersModelView.MateriaId = registroNotaHeader.MateriaId;
                RegistroNotaHeadersModelView.ModuloId = registroNotaHeader.ModuloId;
                RegistroNotaHeadersModelView.InstructorId = registroNotaHeader.InstructorId;                
                RegistroNotaHeadersModelView.PorcentajeDominioTotal = registroNotaHeader.PorcentajeDominioTotal;
                RegistroNotaHeadersModelView.PorcentajeProgresoTotal = registroNotaHeader.PorcentajeProgresoTotal;
            }
        }

        public async Task<IActionResult> OnPostEditRegistroNotaEstudianteHeadersAsync(int grupoId, int materiaId, int registroNotaHeaderId)
        {
            RegistroNotaHeaderId = registroNotaHeaderId;
            if (!ModelState.IsValid)
            {
                await SetExtraData(grupoId, materiaId);
                await FillSelectListsItems();
                await SetRegistroNotaEstudianteHeaders(registroNotaHeaderId);

                Message = "Por favor complete el formulario correctamente";

                return Page();
            }

            var porcentajeTotal = RegistroNotaHeadersModelView.PorcentajeProgresoTotal + RegistroNotaHeadersModelView.PorcentajeDominioTotal;
            if (porcentajeTotal != 100)
            {
                await SetExtraData(grupoId, materiaId);
                await FillSelectListsItems();
                await SetRegistroNotaEstudianteHeaders(registroNotaHeaderId);

                Message = "La suma de porcentajes debe ser 100";

                return Page();
            }

            // ProgramaId 1 es TMA
            RegistroNotaHeadersModelView.ProgramaId = 1;
            RegistroNotaHeadersModelView.GrupoId = grupoId;
            RegistroNotaHeadersModelView.MateriaId = materiaId;

            var moduloMateriaServiceApi = GetIModuloMateriaServiceApi();
            var moduloMateriaServiceResponse = await moduloMateriaServiceApi.GetModuloByMateriaAsync(materiaId);
            if (moduloMateriaServiceResponse.IsSuccessStatusCode)
            {
                RegistroNotaHeadersModelView.ModuloId = moduloMateriaServiceResponse.Content.ModuloId;
            }
            else
            {
                await SetExtraData(grupoId, materiaId);
                await FillSelectListsItems();
                await SetRegistroNotaEstudianteHeaders(registroNotaHeaderId);

                Message = "Ocurrio un error, intente nuevamente.";

                return Page();
            }

            var registroNotaHeaderServiceApi = GetIRegistroNotaHeaderServiceApi();
            var updateRegistroNotaHeader = new CIAC_TAS_Service.Contracts.V1.Requests.UpdateRegistroNotaHeaderRequest
            {
                ProgramaId = RegistroNotaHeadersModelView.ProgramaId,
                GrupoId = RegistroNotaHeadersModelView.GrupoId,
                MateriaId = RegistroNotaHeadersModelView.MateriaId,
                ModuloId = RegistroNotaHeadersModelView.ModuloId,
                InstructorId = RegistroNotaHeadersModelView.InstructorId,
                IsLocked = false,
                PorcentajeDominioTotal = RegistroNotaHeadersModelView.PorcentajeDominioTotal,
                PorcentajeProgresoTotal = RegistroNotaHeadersModelView.PorcentajeProgresoTotal
            };

            var registroNotaHeaderResponse = await registroNotaHeaderServiceApi.UpdateAsync(registroNotaHeaderId, updateRegistroNotaHeader);
            if (!registroNotaHeaderResponse.IsSuccessStatusCode)
            {
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(registroNotaHeaderResponse.Error.Content);

                Message = String.Join(" ", errorResponse.Errors.Select(x => x.Message));

                await SetExtraData(grupoId, materiaId);
                await FillSelectListsItems();
                await SetRegistroNotaEstudianteHeaders(registroNotaHeaderId);

                return Page();
            }

            return RedirectToPage("/Instructor/RegistroNotasHeaders", new { grupoId = grupoId, materiaId = materiaId });
        }

        private async Task SetRegistroNotaEstudianteHeaders(int registroNotaHeaderId)
        {
            var registroNotaEstudianteHeaderServiceApi = GetIRegistroNotaEstudianteHeaderServiceApi();
            var registroNotaEstudianteHeaderResponse = await registroNotaEstudianteHeaderServiceApi.GetAllByRegistroNotaHeaderIdasync(registroNotaHeaderId);

            if (registroNotaEstudianteHeaderResponse.IsSuccessStatusCode)
            {
                RegistroNotaEstudianteHeadersModelView = registroNotaEstudianteHeaderResponse.Content.Data.Select(
                    x => new RegistroNotaEstudianteHeadersModelView
                    {
                        Id = x.Id,
                        EstudianteId = x.EstudianteId,
                        EstudianteNombre = x.Estudiante.Nombre + " " + x.Estudiante.ApellidoPaterno
                    }).ToList();
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

        private async Task FillSelectListsItems()
        {
            var instructorServiceApi = GetIInstructorServiceApi();
            var instructorResponse = await instructorServiceApi.GetAllAsync();

            if (instructorResponse.IsSuccessStatusCode)
            {
                InstructorOptions = instructorResponse.Content.Data.Select(x => new SelectListItem { Text = x.Nombres + " " + x.ApellidoPaterno, Value = x.Id.ToString() }).ToList();
            }
        }

        private IModuloMateriaServiceApi GetIModuloMateriaServiceApi()
        {
            return RestService.For<IModuloMateriaServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
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

        private IInstructorServiceApi GetIInstructorServiceApi()
        {
            return RestService.For<IInstructorServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
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

        private IRegistroNotaEstudianteHeaderServiceApi GetIRegistroNotaEstudianteHeaderServiceApi()
        {
            return RestService.For<IRegistroNotaEstudianteHeaderServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
        }

        private IRegistroNotaHeaderServiceApi GetIRegistroNotaHeaderServiceApi()
        {
            return RestService.For<IRegistroNotaHeaderServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
        }
    }
}
