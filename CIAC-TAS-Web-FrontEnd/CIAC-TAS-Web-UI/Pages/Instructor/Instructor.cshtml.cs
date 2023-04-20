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
    public class InstructorModel : PageModel
    {
		[BindProperty]
		public InstructorModelView InstructorModelView { get; set; }

		public List<SelectListItem> UsuariosOptions { get; set; }
		public List<SelectListItem> VacunaAntitetanicaOptions { get; set; }


		[TempData]
		public string Message { get; set; }

		private readonly IConfiguration _configuration;
		public InstructorModel(IConfiguration configuration)
		{
			_configuration = configuration;
			VacunaAntitetanicaOptions = PredefinedListsHelper.YesNo.YesNoOptions;
		}

		public async Task OnGetNewInstructorAsync()
		{
			var identityApi = GetIIdentityApi();
			var usersResponse = await identityApi.GetUsersByRoleNameAsync(RolesHelper.INSTRUCTOR_ROLE);

			InstructorModelView = new InstructorModelView();
			InstructorModelView.Fecha = DateTime.Today;
			InstructorModelView.FechaNacimiento = DateTime.Today;

			if (usersResponse.IsSuccessStatusCode)
			{
				UsuariosOptions = usersResponse.Content.Data.Select(x => new SelectListItem { Text = x.UserName, Value = x.Id }).ToList();
			}
		}

		public async Task<IActionResult> OnPostNewInstructorAsync()
		{
			if (!ModelState.IsValid)
			{
				Message = "Por favor complete el formulario correctamente";

				var identityApi = GetIIdentityApi();
				var usersResponse = await identityApi.GetUsersByRoleNameAsync(RolesHelper.INSTRUCTOR_ROLE);

				if (usersResponse.IsSuccessStatusCode)
				{
					UsuariosOptions = usersResponse.Content.Data.Select(x => new SelectListItem { Text = x.UserName, Value = x.Id }).ToList();
				}

				return Page();
			}

			var instructorApi = GetIInstructorServiceApi();
			var instructorRequest = new CIAC_TAS_Service.Contracts.V1.Requests.CreateInstructorRequest
			{
				UserId = InstructorModelView.UserId,
				NumeroLicencia = InstructorModelView.NumeroLicencia,
				CodigoTas = InstructorModelView.CodigoTas,
				Fecha = InstructorModelView.Fecha,
				Nombres = InstructorModelView.Nombres,
				ApellidoPaterno = InstructorModelView.ApellidoPaterno,
				ApellidoMaterno = InstructorModelView.ApellidoMaterno,
				FechaNacimiento = InstructorModelView.FechaNacimiento,				
				Nacionalidad = InstructorModelView.Nacionalidad,
				EstadoCivil = InstructorModelView.EstadoCivil,
				Domicilio = InstructorModelView.Domicilio,
				Telefono = InstructorModelView.Telefono,
				Celular = InstructorModelView.Celular,
				Email = InstructorModelView.Email,
				Formacion = InstructorModelView.Formacion,
				Cursos = InstructorModelView.Cursos,
				ExperienciaLaboral = InstructorModelView.ExperienciaLaboral,
				ExperienciaInstruccion = InstructorModelView.ExperienciaInstruccion,
				VacunaAntitetanica = InstructorModelView.VacunaAntitetanica
			};

			var instructorResponse = await instructorApi.CreateAsync(instructorRequest);
			if (!instructorResponse.IsSuccessStatusCode)
			{
				var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(instructorResponse.Error.Content);

				Message = String.Join(" ", errorResponse.Errors.Select(x => x.Message));

				var identityApi = GetIIdentityApi();
				var usersResponse = await identityApi.GetUsersByRoleNameAsync(RolesHelper.INSTRUCTOR_ROLE);

				if (usersResponse.IsSuccessStatusCode)
				{
					UsuariosOptions = usersResponse.Content.Data.Select(x => new SelectListItem { Text = x.UserName, Value = x.Id }).ToList();
				}

				return Page();
			}

			return RedirectToPage("/Instructor/Instructores");
		}

		public async Task<IActionResult> OnGetEditInstructorAsync(int id)
		{
			var instructorApi = GetIInstructorServiceApi();
			var InstructorResponse = await instructorApi.GetAsync(id);

			if (!InstructorResponse.IsSuccessStatusCode)
			{
				Message = "Ocurrio un error inesperado";

				return RedirectToPage("/Instructor/Instructores");
			}

			var identityApi = GetIIdentityApi();
			var usersResponse = await identityApi.GetUsersByRoleNameAsync(RolesHelper.INSTRUCTOR_ROLE);

			if (usersResponse.IsSuccessStatusCode)
			{
				UsuariosOptions = usersResponse.Content.Data.Select(x => new SelectListItem { Text = x.UserName, Value = x.Id }).ToList();
			}

			var instructor = InstructorResponse.Content;
			InstructorModelView = new InstructorModelView
			{
				Id = instructor.Id,
				UserId = instructor.UserId,
				NumeroLicencia = instructor.NumeroLicencia,
				CodigoTas = instructor.CodigoTas,
				Fecha = instructor.Fecha,
				Nombres = instructor.Nombres,
				ApellidoPaterno = instructor.ApellidoPaterno,
				ApellidoMaterno = instructor.ApellidoMaterno,
				FechaNacimiento = instructor.FechaNacimiento,
				Nacionalidad = instructor.Nacionalidad,
				EstadoCivil = instructor.EstadoCivil,
				Domicilio = instructor.Domicilio,
				Telefono = instructor.Telefono,
				Celular = instructor.Celular,
				Email = instructor.Email,
				Formacion = instructor.Formacion,
				Cursos = instructor.Cursos,
				ExperienciaLaboral = instructor.ExperienciaLaboral,
				ExperienciaInstruccion = instructor.ExperienciaInstruccion,
				VacunaAntitetanica = instructor.VacunaAntitetanica
			};

			return Page();
		}
		public async Task<IActionResult> OnPostEditInstructorAsync(int id)
		{
			if (!ModelState.IsValid)
			{
				Message = "Por favor complete el formulario correctamente";

				var identityApi = GetIIdentityApi();
				var usersResponse = await identityApi.GetUsersByRoleNameAsync(RolesHelper.INSTRUCTOR_ROLE);

				if (usersResponse.IsSuccessStatusCode)
				{
					UsuariosOptions = usersResponse.Content.Data.Select(x => new SelectListItem { Text = x.UserName, Value = x.Id }).ToList();
				}

				return Page();
			}

			var InstructorApi = GetIInstructorServiceApi();
			var InstructorRequest = new CIAC_TAS_Service.Contracts.V1.Requests.UpdateInstructorRequest
			{
				UserId = InstructorModelView.UserId,
				NumeroLicencia = InstructorModelView.NumeroLicencia,
				CodigoTas = InstructorModelView.CodigoTas,
				Fecha = InstructorModelView.Fecha,
				Nombres = InstructorModelView.Nombres,
				ApellidoPaterno = InstructorModelView.ApellidoPaterno,
				ApellidoMaterno = InstructorModelView.ApellidoMaterno,
				FechaNacimiento = InstructorModelView.FechaNacimiento,
				Nacionalidad = InstructorModelView.Nacionalidad,
				EstadoCivil = InstructorModelView.EstadoCivil,
				Domicilio = InstructorModelView.Domicilio,
				Telefono = InstructorModelView.Telefono,
				Celular = InstructorModelView.Celular,
				Email = InstructorModelView.Email,
				Formacion = InstructorModelView.Formacion,
				Cursos = InstructorModelView.Cursos,
				ExperienciaLaboral = InstructorModelView.ExperienciaLaboral,
				ExperienciaInstruccion = InstructorModelView.ExperienciaInstruccion,
				VacunaAntitetanica = InstructorModelView.VacunaAntitetanica
			};

			var instructorResponse = await InstructorApi.UpdateAsync(id, InstructorRequest);
			if (!instructorResponse.IsSuccessStatusCode)
			{
				var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(instructorResponse.Error.Content);

				Message = String.Join(" ", errorResponse.Errors.Select(x => x.Message));

				var identityApi = GetIIdentityApi();
				var usersResponse = await identityApi.GetUsersByRoleNameAsync(RolesHelper.INSTRUCTOR_ROLE);

				if (usersResponse.IsSuccessStatusCode)
				{
					UsuariosOptions = usersResponse.Content.Data.Select(x => new SelectListItem { Text = x.UserName, Value = x.Id }).ToList();
				}

				return Page();
			}

			return RedirectToPage("/Instructor/Instructores");
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

		private IInstructorServiceApi GetIInstructorServiceApi()
		{
			return RestService.For<IInstructorServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
			{
				AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
			});
		}
	}
}
