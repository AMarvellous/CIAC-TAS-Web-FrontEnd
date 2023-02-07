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
    public class EstudianteModel : PageModel
    {
        [BindProperty]
        public EstudianteModelView EstudianteModelView { get; set; }

        public List<SelectListItem> SexoOptions {get; set;}
        public List<SelectListItem> UsuariosOptions {get; set;}
        public List<SelectListItem> VacunaAntitetanicaOptions { get; set; }
        public List<SelectListItem> ExamenPsicofisiologico { get; set; }
        public List<SelectListItem> InstruccionPrevia { get; set; }
        public List<SelectListItem> ExperienciaPrevia { get; set; }

        [TempData]
        public string Message { get; set; }

        private readonly IConfiguration _configuration;
        public EstudianteModel(IConfiguration configuration)
        {
            _configuration = configuration;
            VacunaAntitetanicaOptions = PredefinedListsHelper.YesNo.YesNoOptions;
            ExamenPsicofisiologico = PredefinedListsHelper.YesNo.YesNoOptions;
            InstruccionPrevia = PredefinedListsHelper.YesNo.YesNoOptions;
            ExperienciaPrevia = PredefinedListsHelper.YesNo.YesNoOptions;
        }

        public async Task OnGetNewEstudianteAsync()
        {
            var identityApi = GetIIdentityApi();
            var usersResponse = await identityApi.GetUsersAsync();

            if (usersResponse.IsSuccessStatusCode)
            {
                UsuariosOptions = usersResponse.Content.Data.Select(x => new SelectListItem { Text = x.UserName, Value = x.Id }).ToList();
            }
        }

        public async Task<IActionResult> OnPostNewEstudianteAsync()
        {
            if (!ModelState.IsValid)
            {
                Message = "Por favor complete el formulario correctamente";

                var identityApi = GetIIdentityApi();
                var usersResponse = await identityApi.GetUsersAsync();

                if (usersResponse.IsSuccessStatusCode)
                {
                    UsuariosOptions = usersResponse.Content.Data.Select(x => new SelectListItem { Text = x.UserName, Value = x.Id }).ToList();
                }

                return Page();
            }

            var estudianteApi = GetIEstudianteServiceApi();
            var estudianteRequest = new CIAC_TAS_Service.Contracts.V1.Requests.CreateEstudianteRequest
            {
                UserId = EstudianteModelView.UserId,
                CarnetIdentidad = EstudianteModelView.CarnetIdentidad,
                CodigoTas = EstudianteModelView.CodigoTas,
                Fecha = EstudianteModelView.Fecha,
                Nombre = EstudianteModelView.Nombre,
                ApellidoPaterno = EstudianteModelView.ApellidoPaterno,
                ApellidoMaterno = EstudianteModelView.ApellidoMaterno,
                LugarNacimiento = EstudianteModelView.LugarNacimiento,
                Sexo = EstudianteModelView.Sexo,
                FechaNacimiento = EstudianteModelView.FechaNacimiento,
                Nacionalidad = EstudianteModelView.Nacionalidad,
                EstadoCivil = EstudianteModelView.EstadoCivil,
                Domicilio = EstudianteModelView.Domicilio,
                Telefono = EstudianteModelView.Telefono,
                Celular = EstudianteModelView.Celular,
                FamiliarTutor = EstudianteModelView.FamiliarTutor,
                Email = EstudianteModelView.Email,
                NombrePadre = EstudianteModelView.NombrePadre,
                CelularPadre = EstudianteModelView.CelularPadre,
                NombreMadre = EstudianteModelView.NombreMadre,
                CelularMadre = EstudianteModelView.CelularMadre,
                NombreTutor = EstudianteModelView.NombreTutor,
                CelularTutor = EstudianteModelView.CelularTutor,
                VacunaAntitetanica = EstudianteModelView.VacunaAntitetanica,
                ExamenPsicofisiologico = EstudianteModelView.ExamenPsicofisiologico,
                CodigoSeguro = EstudianteModelView.CodigoSeguro,
                FechaSeguro = EstudianteModelView.FechaSeguro,
                InstruccionPrevia = EstudianteModelView.InstruccionPrevia,
                ExperienciaPrevia = EstudianteModelView.ExperienciaPrevia
            };

            var estudianteResponse = await estudianteApi.CreateAsync(estudianteRequest);
            if (!estudianteResponse.IsSuccessStatusCode)
            {
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(estudianteResponse.Error.Content);

                Message = String.Join(" ", errorResponse.Errors.Select(x => x.Message));

                var identityApi = GetIIdentityApi();
                var usersResponse = await identityApi.GetUsersAsync();

                if (usersResponse.IsSuccessStatusCode)
                {
                    UsuariosOptions = usersResponse.Content.Data.Select(x => new SelectListItem { Text = x.UserName, Value = x.Id }).ToList();
                }

                return Page();
            }

            return RedirectToPage("/Estudiante/Estudiantes");
        }

        public async Task<IActionResult> OnGetEditEstudianteAsync(int id)
        {
            var estudianteApi = GetIEstudianteServiceApi();
            var estudianteResponse = await estudianteApi.GetAsync(id);

            if (!estudianteResponse.IsSuccessStatusCode)
            {
                Message = "Ocurrio un error inesperado";

                return RedirectToPage("/Estudiante/Estudiantes");
            }

            var identityApi = GetIIdentityApi();
            var usersResponse = await identityApi.GetUsersAsync();

            if (usersResponse.IsSuccessStatusCode)
            {
                UsuariosOptions = usersResponse.Content.Data.Select(x => new SelectListItem { Text = x.UserName, Value = x.Id }).ToList();
            }

            var estudiante = estudianteResponse.Content;
            EstudianteModelView = new EstudianteModelView
            {
                Id = estudiante.Id,
                UserId = estudiante.UserId,
                CarnetIdentidad = estudiante.CarnetIdentidad,
                CodigoTas = estudiante.CodigoTas,
                Fecha = estudiante.Fecha,
                Nombre = estudiante.Nombre,
                ApellidoPaterno = estudiante.ApellidoPaterno,
                ApellidoMaterno = estudiante.ApellidoMaterno,
                LugarNacimiento = estudiante.LugarNacimiento,
                Sexo = estudiante.Sexo,
                FechaNacimiento = estudiante.FechaNacimiento,
                Nacionalidad = estudiante.Nacionalidad,
                EstadoCivil = estudiante.EstadoCivil,
                Domicilio = estudiante.Domicilio,
                Telefono = estudiante.Telefono,
                Celular = estudiante.Celular,
                FamiliarTutor = estudiante.FamiliarTutor,
                Email = estudiante.Email,
                NombrePadre = estudiante.NombrePadre,
                CelularPadre = estudiante.CelularPadre,
                NombreMadre = estudiante.NombreMadre,
                CelularMadre = estudiante.CelularMadre,
                NombreTutor = estudiante.NombreTutor,
                CelularTutor = estudiante.CelularTutor,
                VacunaAntitetanica = estudiante.VacunaAntitetanica,
                ExamenPsicofisiologico = estudiante.ExamenPsicofisiologico,
                CodigoSeguro = estudiante.CodigoSeguro,
                FechaSeguro = estudiante.FechaSeguro,
                InstruccionPrevia = estudiante.InstruccionPrevia,
                ExperienciaPrevia = estudiante.ExperienciaPrevia
            };

            return Page();
        }
        public async Task<IActionResult> OnPostEditEstudianteAsync(int id)
        {
            if (!ModelState.IsValid)
            {
                Message = "Por favor complete el formulario correctamente";

                var identityApi = GetIIdentityApi();
                var usersResponse = await identityApi.GetUsersAsync();

                if (usersResponse.IsSuccessStatusCode)
                {
                    UsuariosOptions = usersResponse.Content.Data.Select(x => new SelectListItem { Text = x.UserName, Value = x.Id }).ToList();
                }

                return Page();
            }

            var estudianteApi = GetIEstudianteServiceApi();
            var estudianteRequest = new CIAC_TAS_Service.Contracts.V1.Requests.UpdateEstudianteRequest
            {
                UserId = EstudianteModelView.UserId,
                CarnetIdentidad = EstudianteModelView.CarnetIdentidad,
                CodigoTas = EstudianteModelView.CodigoTas,
                Fecha = EstudianteModelView.Fecha,
                Nombre = EstudianteModelView.Nombre,
                ApellidoPaterno = EstudianteModelView.ApellidoPaterno,
                ApellidoMaterno = EstudianteModelView.ApellidoMaterno,
                LugarNacimiento = EstudianteModelView.LugarNacimiento,
                Sexo = EstudianteModelView.Sexo,
                FechaNacimiento = EstudianteModelView.FechaNacimiento,
                Nacionalidad = EstudianteModelView.Nacionalidad,
                EstadoCivil = EstudianteModelView.EstadoCivil,
                Domicilio = EstudianteModelView.Domicilio,
                Telefono = EstudianteModelView.Telefono,
                Celular = EstudianteModelView.Celular,
                FamiliarTutor = EstudianteModelView.FamiliarTutor,
                Email = EstudianteModelView.Email,
                NombrePadre = EstudianteModelView.NombrePadre,
                CelularPadre = EstudianteModelView.CelularPadre,
                NombreMadre = EstudianteModelView.NombreMadre,
                CelularMadre = EstudianteModelView.CelularMadre,
                NombreTutor = EstudianteModelView.NombreTutor,
                CelularTutor = EstudianteModelView.CelularTutor,
                VacunaAntitetanica = EstudianteModelView.VacunaAntitetanica,
                ExamenPsicofisiologico = EstudianteModelView.ExamenPsicofisiologico,
                CodigoSeguro = EstudianteModelView.CodigoSeguro,
                FechaSeguro = EstudianteModelView.FechaSeguro,
                InstruccionPrevia = EstudianteModelView.InstruccionPrevia,
                ExperienciaPrevia = EstudianteModelView.ExperienciaPrevia
            };

            var estudianteResponse = await estudianteApi.UpdateAsync(id, estudianteRequest);
            if (!estudianteResponse.IsSuccessStatusCode)
            {
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(estudianteResponse.Error.Content);

                Message = String.Join(" ", errorResponse.Errors.Select(x => x.Message));

                var identityApi = GetIIdentityApi();
                var usersResponse = await identityApi.GetUsersAsync();

                if (usersResponse.IsSuccessStatusCode)
                {
                    UsuariosOptions = usersResponse.Content.Data.Select(x => new SelectListItem { Text = x.UserName, Value = x.Id }).ToList();
                }

                return Page();
            }

            return RedirectToPage("/Estudiante/Estudiantes");
        }

        private IIdentityApi GetIIdentityApi()
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri(_configuration.GetValue<string>("ServiceUrl")),
                DefaultRequestHeaders = {
                        {"Authorization", $"Bearer {HttpContext.Session.GetString(Session.SessionToken)}"}
                    }
            };
            return RestService.For<IIdentityApi>(client);
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
