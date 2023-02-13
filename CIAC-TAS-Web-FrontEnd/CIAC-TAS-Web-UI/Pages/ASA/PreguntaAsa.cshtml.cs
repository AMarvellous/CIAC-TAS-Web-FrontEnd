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
    public class PreguntAsaModel : PageModel
    {
        [BindProperty]
        public PreguntaAsaView PreguntaAsaModelView { get; set; }

        [BindProperty]
        public IFormFile? UploadFile { get; set; }

        public List<SelectListItem> EstadoPreguntaAsaOptions { get; set; }
        public List<SelectListItem> GrupoPreguntaAsaOptions { get; set; }

        [TempData]
        public string Message { get; set; }

        private readonly IConfiguration _configuration;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment;
        public PreguntAsaModel(IConfiguration configuration, Microsoft.AspNetCore.Hosting.IHostingEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        public async Task OnGetNewPreguntaAsaAsync()
        {
            await FillSelectListsItems();
        }             

        public async Task<IActionResult> OnPostNewPreguntaAsaAsync()
        {
            if (!ModelState.IsValid)
            {
                Message = "Por favor complete el formulario correctamente";

                await FillSelectListsItems();

                return Page();
            }
                        
            if (UploadFile != null && !string.IsNullOrEmpty(UploadFile.FileName))
            {
                var fileName = DateTime.Now.Ticks + "_" + UploadFile.FileName;
                string preguntaAsaContainerPath = Path.Combine(_environment.WebRootPath, "dist/uploads/PreguntaAsa");
                var fullPath = Path.Combine(preguntaAsaContainerPath, fileName);
                PreguntaAsaModelView.Ruta = fileName;
                
                if (!Directory.Exists(preguntaAsaContainerPath))
                {
                    Directory.CreateDirectory(preguntaAsaContainerPath);
                }

                using (var fileStream = new FileStream(fullPath, FileMode.CreateNew))
                {
                    await UploadFile.CopyToAsync(fileStream);
                }
            } else
            {
                PreguntaAsaModelView.Ruta = "";
            }

            var preguntaAsaApi = GetIPreguntaAsaServiceApi();
            var createPreguntaAsaRequest = new CreatePreguntaAsaRequest
            {
                NumeroPregunta = PreguntaAsaModelView.NumeroPregunta,
                Pregunta = PreguntaAsaModelView.Pregunta,
                Ruta = PreguntaAsaModelView.Ruta,
                GrupoPreguntaAsaId = PreguntaAsaModelView.GrupoPreguntaAsaId,
                EstadoPreguntaAsaId = PreguntaAsaModelView.EstadoPreguntaAsaId
            };

            var createPreguntaAsaResponse = await preguntaAsaApi.CreateAsync(createPreguntaAsaRequest);
            if (!createPreguntaAsaResponse.IsSuccessStatusCode)
            {
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(createPreguntaAsaResponse.Error.Content);

                Message = String.Join(" ", errorResponse.Errors.Select(x => x.Message));

                await FillSelectListsItems();

                return Page();
            }

            return RedirectToPage("/ASA/PreguntasAsa");
        }

        public async Task<IActionResult> OnGetEditPreguntaAsaAsync(int id)
        {
            var preguntaAsaApi = GetIPreguntaAsaServiceApi();
            var preguntaAsaResponse = await preguntaAsaApi.GetAsync(id);

            if (!preguntaAsaResponse.IsSuccessStatusCode)
            {
                Message = "Ocurrio un error inesperado";

                return RedirectToPage("/ASA/PreguntasAsa");
            }

            await FillSelectListsItems();

            var preguntaAsa = preguntaAsaResponse.Content;
            PreguntaAsaModelView = new PreguntaAsaView
            {
                Id = preguntaAsa.Id,
                NumeroPregunta = preguntaAsa.NumeroPregunta,
                Pregunta = preguntaAsa.Pregunta,
                Ruta = preguntaAsa.Ruta,
                GrupoPreguntaAsaId = preguntaAsa.GrupoPreguntaAsaId,
                EstadoPreguntaAsaId = preguntaAsa.EstadoPreguntaAsaId,
                PreguntaAsaOpcionesModelViews = preguntaAsa.PreguntaAsaOpcionesResponse.Select(
                        x => new PreguntaAsaOpcionModelView {
                            Id = x.Id, Opcion = x.Opcion, Texto = x.Texto, RespuestaValida = x.RespuestaValida, PreguntaAsaId = x.PreguntaAsaId
                        }).ToList()
            };

            return Page();
        }

        public async Task<IActionResult> OnPostEditPreguntaAsaAsync(int id)
        {
            if (!ModelState.IsValid)
            {
                Message = "Por favor complete el formulario correctamente";

                await FillSelectListsItems();

                return Page();
            }

            if (UploadFile != null && !string.IsNullOrEmpty(UploadFile.FileName))
            {
                var fileName = DateTime.Now.Ticks + "_" + UploadFile.FileName;
                string preguntaAsaContainerPath = Path.Combine(_environment.WebRootPath, "dist/uploads/PreguntaAsa");
                var fullPath = Path.Combine(preguntaAsaContainerPath, fileName);
                PreguntaAsaModelView.Ruta = fileName;

                if (!Directory.Exists(preguntaAsaContainerPath))
                {
                    Directory.CreateDirectory(preguntaAsaContainerPath);
                }

                using (var fileStream = new FileStream(fullPath, FileMode.CreateNew))
                {
                    await UploadFile.CopyToAsync(fileStream);
                }
            }
            else if (PreguntaAsaModelView.Ruta == null)
            {
                PreguntaAsaModelView.Ruta = "";
            }

            var preguntaAsaServiceApi = GetIPreguntaAsaServiceApi();
            var preguntaAsaRequest = new UpdatePreguntaAsaRequest
            {
                NumeroPregunta = PreguntaAsaModelView.NumeroPregunta,
                Pregunta = PreguntaAsaModelView.Pregunta,
                Ruta = PreguntaAsaModelView.Ruta,
                EstadoPreguntaAsaId = PreguntaAsaModelView.EstadoPreguntaAsaId,
                GrupoPreguntaAsaId = PreguntaAsaModelView.GrupoPreguntaAsaId
            };

            var preguntaAsaResponse = await preguntaAsaServiceApi.UpdateAsync(id, preguntaAsaRequest);
            if (!preguntaAsaResponse.IsSuccessStatusCode)
            {
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(preguntaAsaResponse.Error.Content);

                Message = String.Join(" ", errorResponse.Errors.Select(x => x.Message));

                await FillSelectListsItems();

                return Page();
            }

            return RedirectToPage("/ASA/PreguntasAsa");
        }

        public async Task<IActionResult> OnGetRemovePreguntaAsaOpcionAsync(int preguntaAsaId, int preguntaAsaOpcionId)
        {
            var preguntaAsaOpcionServiceApi = GetIPreguntaAsaOpcionServiceApi();
            var preguntaAsaOpcionResponse = await preguntaAsaOpcionServiceApi.DeleteAsync(preguntaAsaOpcionId);

            if (!preguntaAsaOpcionResponse.IsSuccessStatusCode)
            {
                Message = "Ocurrio un error inesperado";

                return RedirectToPage("/ASA/PreguntaAsa", "EditPreguntaAsa", new { id = preguntaAsaId });
            }

            return RedirectToPage("/ASA/PreguntaAsa", "EditPreguntaAsa", new { id = preguntaAsaId });
        }


        private IPreguntaAsaServiceApi GetIPreguntaAsaServiceApi()
        {
            return RestService.For<IPreguntaAsaServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
        }

        private IPreguntaAsaOpcionServiceApi GetIPreguntaAsaOpcionServiceApi()
        {
            return RestService.For<IPreguntaAsaOpcionServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
        }

        private IGrupoPreguntaAsaServiceApi GetIGrupoPreguntaAsaServiceApi()
        {
            return RestService.For<IGrupoPreguntaAsaServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
        }

        private IEstadoPreguntaAsaServiceApi GetIEstadoPreguntaAsaServiceApi()
        {
            return RestService.For<IEstadoPreguntaAsaServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
        }

        private async Task FillSelectListsItems()
        {
            var grupoPreguntaAsaServiceApi = GetIGrupoPreguntaAsaServiceApi();
            var grupoPreguntaAsaResponse = await grupoPreguntaAsaServiceApi.GetAllAsync();

            if (grupoPreguntaAsaResponse.IsSuccessStatusCode)
            {
                GrupoPreguntaAsaOptions = grupoPreguntaAsaResponse.Content.Data.Select(x => new SelectListItem { Text = x.Nombre, Value = x.Id.ToString() }).ToList();
            }

            var estadoPreguntaAsaServiceApi = GetIEstadoPreguntaAsaServiceApi();
            var estadoPreguntaAsaResponse = await estadoPreguntaAsaServiceApi.GetAllAsync();

            if (estadoPreguntaAsaResponse.IsSuccessStatusCode)
            {
                EstadoPreguntaAsaOptions = estadoPreguntaAsaResponse.Content.Data.Select(x => new SelectListItem { Text = x.Estado.ToString(), Value = x.Id.ToString() }).ToList();
            }
        }
    }
}
