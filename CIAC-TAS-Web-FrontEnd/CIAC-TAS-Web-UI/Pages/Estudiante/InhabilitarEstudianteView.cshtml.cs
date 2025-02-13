using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.Helper;
using CIAC_TAS_Web_UI.ModelViews.Estudiante;
using CIAC_TAS_Web_UI.ModelViews.Instructores;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Refit;

namespace CIAC_TAS_Web_UI.Pages.Estudiante
{
    public class InhabilitarEstudianteViewModel : PageModel
    {
        [BindProperty]
        public List<InhabilitarEstudianteModelView> InhabilitarEstudianteModelView { get; set; } = new List<InhabilitarEstudianteModelView>();
        public string MotivoForm { get; set; }
        public int EstudianteSelected { get; set; }
        public List<SelectListItem> EstudianteOptions { get; set; }
        [TempData]
        public string Message { get; set; }
        private readonly IConfiguration _configuration;

        public InhabilitarEstudianteViewModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task OnGetAsync()
        {
            var estudianteServiceApi = GetEstudianteServiceApi();
            var estudianteResponse = await estudianteServiceApi.GetAllNotAssignedInhabilitacionEstudianteAsync();

            if (estudianteResponse.IsSuccessStatusCode)
            {
                EstudianteOptions = estudianteResponse.Content.Data.Select(x => new SelectListItem
                {
                    Text = x.Nombre + " " + x.ApellidoPaterno,
                    Value = x.Id.ToString(),
                }).ToList();
            }

            var inhabilitacionEstudianteServiceApi = GetIInhabilitacionEstudianteServiceApi();
            var inhabilitacionEstudianteResponse = await inhabilitacionEstudianteServiceApi.GetAllAsync();

            if (inhabilitacionEstudianteResponse.IsSuccessStatusCode)
            {
                InhabilitarEstudianteModelView = inhabilitacionEstudianteResponse.Content.Data.Select(x => new InhabilitarEstudianteModelView
                {
                    Id = x.Id,
                    EstudianteId = x.EstudianteId,
                    Motivo = x.Motivo,
                    EstudianteNombre = x.Estudiante.Nombre + " " + x.Estudiante.ApellidoPaterno
                }).ToList();
            }
        }

        public async Task<JsonResult> OnGetAddInhabilitarEstudianteViewAsync(int estudianteId, string motivo)
        {
            var inhabilitacionEstudianteServiceApi = GetIInhabilitacionEstudianteServiceApi();
            var inhabilitacionEstudianteResponse = await inhabilitacionEstudianteServiceApi.CreateAsync(new CIAC_TAS_Service.Contracts.V1.Requests.CreateInhabilitacionEstudianteRequest
            {
                EstudianteId = estudianteId,
                Motivo = motivo,
            });

            if (!inhabilitacionEstudianteResponse.IsSuccessStatusCode)
            {
                return new JsonResult("Error al intentar Inhabilitar un Estudiante");
            }

            return new JsonResult("Creacion correcta");
        }

        public async Task<IActionResult> OnGetRemoveInhabilitacionEstudianteAsync(int inhabilitacionEstudianteId)
        {
            var inhabilitacionEstudianteServiceApi = GetIInhabilitacionEstudianteServiceApi();
            await inhabilitacionEstudianteServiceApi.DeleteAsync(inhabilitacionEstudianteId);

            return RedirectToPage("/Estudiante/InhabilitarEstudianteView");
        }

        private IInhabilitacionEstudianteServiceApi GetIInhabilitacionEstudianteServiceApi()
        {
            return RestService.For<IInhabilitacionEstudianteServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
        }

        private IEstudianteServiceApi GetEstudianteServiceApi()
        {
            return RestService.For<IEstudianteServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
        }
    }
}
