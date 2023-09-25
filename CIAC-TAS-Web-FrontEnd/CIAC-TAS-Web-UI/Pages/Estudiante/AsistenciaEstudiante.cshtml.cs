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
using static CIAC_TAS_Service.Contracts.V1.ApiRoute;

namespace CIAC_TAS_Web_UI.Pages.Estudiante
{
    public class AsistenciaEstudianteModel : PageModel
    {
        [BindProperty]
        public AsistenciaEstudianteModelView AsistenciaEstudianteModelView { get; set; }
        public List<SelectListItem> EstudianteOptions { get; set; }
        public List<SelectListItem> TipoAsistenciaOptions { get; set; }

        public int GrupoId { get; set; }
        public int MateriaId { get; set; }
        public string GrupoNombre { get; set; }
        public string MateriaNombre { get; set; }

        [TempData]
        public string Message { get; set; }
        private readonly IConfiguration _configuration;

        public AsistenciaEstudianteModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> OnGetNewAsistenciaEstudianteAsync(int grupoId, int materiaId, int asistenciaEstudianteHeaderId)
        {
            await SetExtraData(grupoId, materiaId);

            await FillSelectListsItems(materiaId, grupoId, asistenciaEstudianteHeaderId);

            return Page();
        }

        public async Task<IActionResult> OnPostNewAsistenciaEstudianteAsync(int asistenciaEstudianteHeaderId, int grupoId, int materiaId)
        {
            if (!ModelState.IsValid)
            {
                await SetExtraData(grupoId, materiaId);
                Message = "Por favor complete el formulario correctamente";
                await FillSelectListsItems(materiaId, grupoId, asistenciaEstudianteHeaderId);

                return Page();
            }

            var asistenciaEstudianteApi = GetIAsistenciaEstudianteServiceApi();
            var createAsistenciaEstudianteRequest = new CreateAsistenciaEstudianteRequest
            {
                EstudianteId = AsistenciaEstudianteModelView.EstudianteId,
                AsistenciaEstudianteHeaderId = asistenciaEstudianteHeaderId,
                TipoAsistenciaId = AsistenciaEstudianteModelView.TipoAsistenciaId
            };

            var createAsistenciaEstudianteResponse = await asistenciaEstudianteApi.CreateAsync(createAsistenciaEstudianteRequest);
            if (!createAsistenciaEstudianteResponse.IsSuccessStatusCode)
            {
                await SetExtraData(grupoId, materiaId);
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(createAsistenciaEstudianteResponse.Error.Content);

                Message = String.Join(" ", errorResponse.Errors.Select(x => x.Message));
                await FillSelectListsItems(materiaId, grupoId, asistenciaEstudianteHeaderId);

                return Page();
            }

            return RedirectToPage("/Estudiante/AsistenciaEstudianteHeader", "EditAsistenciaEstudianteHeader", new { id = asistenciaEstudianteHeaderId, grupoId = grupoId, materiaId = materiaId });
        }

        //public async Task<IActionResult> OnGetEditAsistenciaEstudianteAsync(int id, int asistenciaEstudianteHeaderId)
        //{
        //    var asistenciaEstudianteApi = GetIAsistenciaEstudianteServiceApi();
        //    var asistenciaEstudianteResponse = await asistenciaEstudianteApi.GetAsync(id);

        //    if (!asistenciaEstudianteResponse.IsSuccessStatusCode)
        //    {
        //        Message = "Ocurrio un error inesperado";

        //        return RedirectToPage("/Estudiante/AsistenciaEstudianteHeader", "EditAsistenciaEstudianteHeader", new { id = asistenciaEstudianteHeaderId });
        //    }

        //    var asistenciaEstudiante = asistenciaEstudianteResponse.Content;
        //    AsistenciaEstudianteModelView = new AsistenciaEstudianteModelView
        //    {
        //        Id = asistenciaEstudiante.Id,
        //        EstudianteId = asistenciaEstudiante.EstudianteId,
        //    };

        //    return Page();
        //}

        //public async Task<IActionResult> OnPostEditAsistenciaEstudianteAsync(int id, int asistenciaEstudianteHeaderId)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        Message = "Por favor complete el formulario correctamente";

        //        return Page();
        //    }

        //    var asistenciaEstudianteApi = GetIAsistenciaEstudianteServiceApi();
        //    var asistenciaEstudianteRequest = new UpdateAsistenciaEstudianteRequest
        //    {
        //        EstudianteId = AsistenciaEstudianteModelView.EstudianteId,
        //        AsistenciaEstudianteHeaderId = asistenciaEstudianteHeaderId
        //    };

        //    var asistenciaEstudianteResponse = await asistenciaEstudianteApi.UpdateAsync(id, asistenciaEstudianteRequest);
        //    if (!asistenciaEstudianteResponse.IsSuccessStatusCode)
        //    {
        //        var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(asistenciaEstudianteResponse.Error.Content);

        //        Message = String.Join(" ", errorResponse.Errors.Select(x => x.Message));

        //        return Page();
        //    }

        //    return RedirectToPage("/Estudiante/AsistenciaEstudianteHeader", "EditAsistenciaEstudianteHeader", new { id = asistenciaEstudianteHeaderId });
        //}

        private IAsistenciaEstudianteServiceApi GetIAsistenciaEstudianteServiceApi()
        {
            return RestService.For<IAsistenciaEstudianteServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
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

        private ITipoAsistenciaServiceApi GetITipoAsistenciaServiceApi()
        {
            return RestService.For<ITipoAsistenciaServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
        }

        private async Task FillSelectListsItems(int materiaId, int grupoId, int asistenciaEstudianteHeaderId)
        {
            var estudianteServiceApi = GetIEstudianteServiceApi();
            var estudianteResponse = await estudianteServiceApi.GetAllNotAssignedAsistenciaEstudianteAsync(materiaId, grupoId, asistenciaEstudianteHeaderId);

            if (estudianteResponse.IsSuccessStatusCode)
            {
                EstudianteOptions = estudianteResponse.Content.Data.Select(x => new SelectListItem { Text = x.Nombre, Value = x.Id.ToString() }).ToList();
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
