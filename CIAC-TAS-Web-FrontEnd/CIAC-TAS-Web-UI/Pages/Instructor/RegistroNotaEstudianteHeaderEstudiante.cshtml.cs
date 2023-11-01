using CIAC_TAS_Service.Contracts.V1.Responses;
using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.Helper;
using CIAC_TAS_Web_UI.ModelViews.Instructores;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Refit;

namespace CIAC_TAS_Web_UI.Pages.Instructor
{
    public class RegistroNotaEstudianteHeaderEstudianteModel : PageModel
    {
        [BindProperty]
        public RegistroNotaEstudianteHeaderModelView RegistroNotaEstudianteHeaderModelView { get; set; } = new RegistroNotaEstudianteHeaderModelView();
        public int GrupoId { get; set; }
        public int MateriaId { get; set; }
        public string GrupoNombre { get; set; }
        public string MateriaNombre { get; set; }
        public int RegistroNotaHeaderId { get; set; }
        public List<SelectListItem> EstudianteOptions { get; set; }
        [TempData]
        public string Message { get; set; }

        private readonly IConfiguration _configuration;

        public RegistroNotaEstudianteHeaderEstudianteModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task OnGetNewEstudianteRegistroNotaEstudianteHeaderAsync(int grupoId, int materiaId, int registroNotaHeaderId)
        {
            RegistroNotaHeaderId = registroNotaHeaderId;
            await SetExtraData(grupoId, materiaId);
            await FillSelectListsItems(registroNotaHeaderId);
        }

        public async Task<IActionResult> OnPostAddNewEstudianteRegistroNotaEstudianteHeaderAsync(int grupoId, int materiaId, int registroNotaHeaderId)
        {
            RegistroNotaHeaderId = registroNotaHeaderId;
            if (!ModelState.IsValid)
            {
                await SetExtraData(grupoId, materiaId);
                await FillSelectListsItems(registroNotaHeaderId);

                Message = "Por favor complete el formulario correctamente";

                return Page();
            }

            var registroNotaEstudianteHeaderServiceApi = GetIRegistroNotaEstudianteHeaderServiceApi();
            var registroNotaEstudianteHeaderResponse = await registroNotaEstudianteHeaderServiceApi.CreateAsync(new CIAC_TAS_Service.Contracts.V1.Requests.CreateRegistroNotaEstudianteHeaderRequest
            {
                EstudianteId = RegistroNotaEstudianteHeaderModelView.EstudianteId,
                RegistroNotaHeaderId = registroNotaHeaderId,
            });

            if (!registroNotaEstudianteHeaderResponse.IsSuccessStatusCode)
            {
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(registroNotaEstudianteHeaderResponse.Error.Content);

                Message = String.Join(" ", errorResponse.Errors.Select(x => x.Message));

                await SetExtraData(grupoId, materiaId);
                await FillSelectListsItems(registroNotaHeaderId);

                return Page();
            }

            return RedirectToPage("/Instructor/RegistroNotaEstudianteHeaders", "ViewRegistroNotaEstudianteHeaders", new { registroNotaHeaderId = registroNotaHeaderId, grupoId = grupoId, materiaId = materiaId });
        }

        private async Task FillSelectListsItems(int registroNotaHeaderId)
        {
            var estudianteServiceApi = GetIEstudianteServiceApi();
            var estudianteResponse = await estudianteServiceApi.GetAllNotAssignedToRegistroNotaEstudianteHeaderAsync(registroNotaHeaderId);

            if (estudianteResponse.IsSuccessStatusCode)
            {
                EstudianteOptions = estudianteResponse.Content.Data.Select(x => new SelectListItem { Text = x.Nombre + " " + x.ApellidoPaterno, Value = x.Id.ToString() }).ToList();
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

        private IEstudianteServiceApi GetIEstudianteServiceApi()
        {
            return RestService.For<IEstudianteServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
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
    }
}
