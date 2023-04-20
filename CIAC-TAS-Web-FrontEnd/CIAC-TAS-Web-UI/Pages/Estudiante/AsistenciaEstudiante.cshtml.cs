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
    public class AsistenciaEstudianteModel : PageModel
    {
        [BindProperty]
        public AsistenciaEstudianteModelView AsistenciaEstudianteModelView { get; set; }
        public List<SelectListItem> EstudianteOptions { get; set; }

        [TempData]
        public string Message { get; set; }
        private readonly IConfiguration _configuration;

        public AsistenciaEstudianteModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> OnGetNewAsistenciaEstudianteAsync()
        {
            await FillSelectListsItems();

            return Page();
        }

        public async Task<IActionResult> OnPostNewAsistenciaEstudianteAsync(int asistenciaEstudianteHeaderId)
        {
            if (!ModelState.IsValid)
            {
                Message = "Por favor complete el formulario correctamente";
                await FillSelectListsItems();

                return Page();
            }

            var asistenciaEstudianteApi = GetIAsistenciaEstudianteServiceApi();
            var createAsistenciaEstudianteRequest = new CreateAsistenciaEstudianteRequest
            {
                EstudianteId = AsistenciaEstudianteModelView.EstudianteId,
                AsistenciaEstudianteHeaderId = asistenciaEstudianteHeaderId,
            };

            var createAsistenciaEstudianteResponse = await asistenciaEstudianteApi.CreateAsync(createAsistenciaEstudianteRequest);
            if (!createAsistenciaEstudianteResponse.IsSuccessStatusCode)
            {
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(createAsistenciaEstudianteResponse.Error.Content);

                Message = String.Join(" ", errorResponse.Errors.Select(x => x.Message));
                await FillSelectListsItems();

                return Page();
            }

            return RedirectToPage("/Estudiante/AsistenciaEstudianteHeader", "EditAsistenciaEstudianteHeader", new { id = asistenciaEstudianteHeaderId });
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

        private async Task FillSelectListsItems()
        {
            var estudianteServiceApi = GetIEstudianteServiceApi();
            var estudianteResponse = await estudianteServiceApi.GetAllAsync();

            if (estudianteResponse.IsSuccessStatusCode)
            {
                EstudianteOptions = estudianteResponse.Content.Data.Select(x => new SelectListItem { Text = x.Nombre, Value = x.Id.ToString() }).ToList();
            }
        }
    }
}
