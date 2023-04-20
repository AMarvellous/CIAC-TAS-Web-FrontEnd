using CIAC_TAS_Service.Contracts.V1.Responses;
using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.Helper;
using CIAC_TAS_Web_UI.ModelViews.General;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Refit;

namespace CIAC_TAS_Web_UI.Pages.General
{
    public class AdministrativoModel : PageModel
    {
		[BindProperty]
		public AdministrativoModelView AdministrativoModelView { get; set; }

		public List<SelectListItem> UsuariosOptions { get; set; }
		public List<SelectListItem> VacunaAntitetanicaOptions { get; set; }


		[TempData]
		public string Message { get; set; }

		private readonly IConfiguration _configuration;
		public AdministrativoModel(IConfiguration configuration)
		{
			_configuration = configuration;
			VacunaAntitetanicaOptions = PredefinedListsHelper.YesNo.YesNoOptions;
		}

		public async Task OnGetNewAdministrativoAsync()
		{
			var identityApi = GetIIdentityApi();
			var usersResponse = await identityApi.GetUsersByRoleNameAsync(RolesHelper.ADMIN_ROLE);

			AdministrativoModelView = new AdministrativoModelView();
			AdministrativoModelView.Fecha = DateTime.Today;
			AdministrativoModelView.FechaNacimiento = DateTime.Today;

			if (usersResponse.IsSuccessStatusCode)
			{
				UsuariosOptions = usersResponse.Content.Data.Select(x => new SelectListItem { Text = x.UserName, Value = x.Id }).ToList();
			}
		}

		public async Task<IActionResult> OnPostNewAdministrativoAsync()
		{
			if (!ModelState.IsValid)
			{
				Message = "Por favor complete el formulario correctamente";

				var identityApi = GetIIdentityApi();
				var usersResponse = await identityApi.GetUsersByRoleNameAsync(RolesHelper.ADMIN_ROLE);

				if (usersResponse.IsSuccessStatusCode)
				{
					UsuariosOptions = usersResponse.Content.Data.Select(x => new SelectListItem { Text = x.UserName, Value = x.Id }).ToList();
				}

				return Page();
			}

			var administrativoApi = GetIAdministrativoServiceApi();
			var administrativoRequest = new CIAC_TAS_Service.Contracts.V1.Requests.CreateAdministrativoRequest
			{
				UserId = AdministrativoModelView.UserId,
				LicenciaCarnetIdentidad = AdministrativoModelView.LicenciaCarnetIdentidad,
				Fecha = AdministrativoModelView.Fecha,
				Nombres = AdministrativoModelView.Nombres,
				ApellidoPaterno = AdministrativoModelView.ApellidoPaterno,
				ApellidoMaterno = AdministrativoModelView.ApellidoMaterno,
				LugarNacimiento = AdministrativoModelView.LugarNacimiento,
				Sexo = AdministrativoModelView.Sexo,
				FechaNacimiento = AdministrativoModelView.FechaNacimiento,
				Nacionalidad = AdministrativoModelView.Nacionalidad,
				EstadoCivil = AdministrativoModelView.EstadoCivil,
				Domicilio = AdministrativoModelView.Domicilio,
				Telefono = AdministrativoModelView.Telefono,
				Celular = AdministrativoModelView.Celular,
				Email = AdministrativoModelView.Email,
				Formacion = AdministrativoModelView.Formacion,
				Cursos = AdministrativoModelView.Cursos,
				ExperienciaLaboral = AdministrativoModelView.ExperienciaLaboral,
				ExperienciaInstruccion = AdministrativoModelView.ExperienciaInstruccion,
				VacunaAntitetanica = AdministrativoModelView.VacunaAntitetanica
			};

			var administrativoResponse = await administrativoApi.CreateAsync(administrativoRequest);
			if (!administrativoResponse.IsSuccessStatusCode)
			{
				var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(administrativoResponse.Error.Content);

				Message = String.Join(" ", errorResponse.Errors.Select(x => x.Message));

				var identityApi = GetIIdentityApi();
				var usersResponse = await identityApi.GetUsersByRoleNameAsync(RolesHelper.ADMIN_ROLE);

				if (usersResponse.IsSuccessStatusCode)
				{
					UsuariosOptions = usersResponse.Content.Data.Select(x => new SelectListItem { Text = x.UserName, Value = x.Id }).ToList();
				}

				return Page();
			}

			return RedirectToPage("/General/Administrativos");
		}

		public async Task<IActionResult> OnGetEditAdministrativoAsync(int id)
		{
			var administrativoApi = GetIAdministrativoServiceApi();
			var administrativoResponse = await administrativoApi.GetAsync(id);

			if (!administrativoResponse.IsSuccessStatusCode)
			{
				Message = "Ocurrio un error inesperado";

				return RedirectToPage("/General/Administrativos");
			}

			var identityApi = GetIIdentityApi();
			var usersResponse = await identityApi.GetUsersByRoleNameAsync(RolesHelper.ADMIN_ROLE);

			if (usersResponse.IsSuccessStatusCode)
			{
				UsuariosOptions = usersResponse.Content.Data.Select(x => new SelectListItem { Text = x.UserName, Value = x.Id }).ToList();
			}

			var administrativo = administrativoResponse.Content;
			AdministrativoModelView = new AdministrativoModelView
			{
				Id = administrativo.Id,
				UserId = administrativo.UserId,
				LicenciaCarnetIdentidad = administrativo.LicenciaCarnetIdentidad,
				Fecha = administrativo.Fecha,
				Nombres = administrativo.Nombres,
				ApellidoPaterno = administrativo.ApellidoPaterno,
				ApellidoMaterno = administrativo.ApellidoMaterno,
				LugarNacimiento = administrativo.LugarNacimiento,
				Sexo = administrativo.Sexo,
				FechaNacimiento = administrativo.FechaNacimiento,
				Nacionalidad = administrativo.Nacionalidad,
				EstadoCivil = administrativo.EstadoCivil,
				Domicilio = administrativo.Domicilio,
				Telefono = administrativo.Telefono,
				Celular = administrativo.Celular,
				Email = administrativo.Email,
				Formacion = administrativo.Formacion,
				Cursos = administrativo.Cursos,
				ExperienciaLaboral = administrativo.ExperienciaLaboral,
				ExperienciaInstruccion = administrativo.ExperienciaInstruccion,
				VacunaAntitetanica = administrativo.VacunaAntitetanica
			};

			return Page();
		}
		public async Task<IActionResult> OnPostEditAdministrativoAsync(int id)
		{
			if (!ModelState.IsValid)
			{
				Message = "Por favor complete el formulario correctamente";

				var identityApi = GetIIdentityApi();
				var usersResponse = await identityApi.GetUsersByRoleNameAsync(RolesHelper.ADMIN_ROLE);

				if (usersResponse.IsSuccessStatusCode)
				{
					UsuariosOptions = usersResponse.Content.Data.Select(x => new SelectListItem { Text = x.UserName, Value = x.Id }).ToList();
				}

				return Page();
			}

			var administrativoApi = GetIAdministrativoServiceApi();
			var administrativoRequest = new CIAC_TAS_Service.Contracts.V1.Requests.UpdateAdministrativoRequest
			{
				UserId = AdministrativoModelView.UserId,
				LicenciaCarnetIdentidad = AdministrativoModelView.LicenciaCarnetIdentidad,
				Fecha = AdministrativoModelView.Fecha,
				Nombres = AdministrativoModelView.Nombres,
				ApellidoPaterno = AdministrativoModelView.ApellidoPaterno,
				ApellidoMaterno = AdministrativoModelView.ApellidoMaterno,
				LugarNacimiento = AdministrativoModelView.LugarNacimiento,
				Sexo = AdministrativoModelView.Sexo,
				FechaNacimiento = AdministrativoModelView.FechaNacimiento,
				Nacionalidad = AdministrativoModelView.Nacionalidad,
				EstadoCivil = AdministrativoModelView.EstadoCivil,
				Domicilio = AdministrativoModelView.Domicilio,
				Telefono = AdministrativoModelView.Telefono,
				Celular = AdministrativoModelView.Celular,
				Email = AdministrativoModelView.Email,
				Formacion = AdministrativoModelView.Formacion,
				Cursos = AdministrativoModelView.Cursos,
				ExperienciaLaboral = AdministrativoModelView.ExperienciaLaboral,
				ExperienciaInstruccion = AdministrativoModelView.ExperienciaInstruccion,
				VacunaAntitetanica = AdministrativoModelView.VacunaAntitetanica
			};

			var administrativoResponse = await administrativoApi.UpdateAsync(id, administrativoRequest);
			if (!administrativoResponse.IsSuccessStatusCode)
			{
				var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(administrativoResponse.Error.Content);

				Message = String.Join(" ", errorResponse.Errors.Select(x => x.Message));

				var identityApi = GetIIdentityApi();
				var usersResponse = await identityApi.GetUsersByRoleNameAsync(RolesHelper.ADMIN_ROLE);

				if (usersResponse.IsSuccessStatusCode)
				{
					UsuariosOptions = usersResponse.Content.Data.Select(x => new SelectListItem { Text = x.UserName, Value = x.Id }).ToList();
				}

				return Page();
			}

			return RedirectToPage("/General/Administrativos");
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

		private IAdministrativoServiceApi GetIAdministrativoServiceApi()
		{
			return RestService.For<IAdministrativoServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
			{
				AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
			});
		}
	}
}
