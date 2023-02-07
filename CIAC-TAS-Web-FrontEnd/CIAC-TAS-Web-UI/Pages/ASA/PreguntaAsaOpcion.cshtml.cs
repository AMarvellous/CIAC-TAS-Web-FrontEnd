using CIAC_TAS_Service.Contracts.V1.Requests;
using CIAC_TAS_Service.Contracts.V1.Responses;
using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.Helper;
using CIAC_TAS_Web_UI.ModelViews.ASA;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Refit;

namespace CIAC_TAS_Web_UI.Pages.ASA
{
    public class PreguntaAsaOpcionModel : PageModel
    {
        [BindProperty]
        public PreguntaAsaOpcionModelView PreguntaAsaOpcionModelView { get; set; }

        [TempData]
        public string Message { get; set; }
        private readonly IConfiguration _configuration;

        public PreguntaAsaOpcionModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> OnGetNewPreguntaAsaOpcionAsync()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostNewPreguntaAsaOpcionAsync(int preguntaAsaId)
        {
            if (!ModelState.IsValid)
            {
                Message = "Por favor complete el formulario correctamente";

                return Page();
            }

            var preguntaAsaOpcionApi = GetIPreguntaAsaOpcionServiceApi();
            var createPreguntaAsaOpcionRequest = new CreatePreguntaAsaOpcionRequest
            {
               Opcion = PreguntaAsaOpcionModelView.Opcion,
               Texto = PreguntaAsaOpcionModelView.Texto,
               RespuestaValida = PreguntaAsaOpcionModelView.RespuestaValida,
               PreguntaAsaId = preguntaAsaId
            };

            var createPreguntaAsaOpcionResponse = await preguntaAsaOpcionApi.CreateAsync(createPreguntaAsaOpcionRequest);
            if (!createPreguntaAsaOpcionResponse.IsSuccessStatusCode)
            {
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(createPreguntaAsaOpcionResponse.Error.Content);

                Message = String.Join(" ", errorResponse.Errors.Select(x => x.Message));

                return Page();
            }

            return RedirectToPage("/ASA/PreguntaAsa", "EditPreguntaAsa", new { id = preguntaAsaId });
        }

        public async Task<IActionResult> OnGetEditPreguntaAsaOpcionAsync(int id, int preguntaAsaId)
        {
            var preguntaAsaOpcionApi = GetIPreguntaAsaOpcionServiceApi();
            var preguntaAsaOpcionResponse = await preguntaAsaOpcionApi.GetAsync(id);

            if (!preguntaAsaOpcionResponse.IsSuccessStatusCode)
            {
                Message = "Ocurrio un error inesperado";

                return RedirectToPage("/ASA/PreguntaAsa", "EditPreguntaAsa", new { id = preguntaAsaId });
            }

            var preguntaAsaOpcion = preguntaAsaOpcionResponse.Content;
            PreguntaAsaOpcionModelView  = new PreguntaAsaOpcionModelView
            {
                Id = preguntaAsaOpcion.Id,
                Opcion = preguntaAsaOpcion.Opcion,
                Texto = preguntaAsaOpcion.Texto,
                RespuestaValida = preguntaAsaOpcion.RespuestaValida,
                PreguntaAsaId = preguntaAsaOpcion.PreguntaAsaId
            };

            return Page();
        }

        public async Task<IActionResult> OnPostEditPreguntaAsaOpcionAsync(int id, int preguntaAsaId)
        {
            if (!ModelState.IsValid)
            {
                Message = "Por favor complete el formulario correctamente";

                return Page();
            }

            var preguntaAsaOpcionApi = GetIPreguntaAsaOpcionServiceApi();
            var preguntaAsaOpcionRequest = new UpdatePreguntaAsaOpcionRequest
            {
                Opcion = PreguntaAsaOpcionModelView.Opcion,
                Texto = PreguntaAsaOpcionModelView.Texto,
                RespuestaValida = PreguntaAsaOpcionModelView.RespuestaValida,
                PreguntaAsaId = preguntaAsaId
            };

            var preguntaAsaOpcionResponse = await preguntaAsaOpcionApi.UpdateAsync(id, preguntaAsaOpcionRequest);
            if (!preguntaAsaOpcionResponse.IsSuccessStatusCode)
            {
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(preguntaAsaOpcionResponse.Error.Content);

                Message = String.Join(" ", errorResponse.Errors.Select(x => x.Message));

                return Page();
            }

            return RedirectToPage("/ASA/PreguntaAsa", "EditPreguntaAsa", new { id = preguntaAsaId });
        }

        private IPreguntaAsaOpcionServiceApi GetIPreguntaAsaOpcionServiceApi()
        {
            return RestService.For<IPreguntaAsaOpcionServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
        }
    }
}
