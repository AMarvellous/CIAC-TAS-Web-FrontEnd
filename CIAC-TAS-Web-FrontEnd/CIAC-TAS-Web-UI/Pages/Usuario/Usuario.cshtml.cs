using CIAC_TAS_Service.Contracts.V1.Responses;
using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.Helper;
using CIAC_TAS_Web_UI.ModelViews.Usuario;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Refit;
using System.Text.Json;

namespace CIAC_TAS_Web_UI.Pages.Usuario
{
    public class UsuarioModel : PageModel
    {
        [BindProperty]
        public UsuarioModelView UsuarioModelView { get; set; } = new UsuarioModelView();

        [BindProperty]
        public bool EditMode { get; set; }

        public List<SelectListItem> Roles { get; set; }

        [TempData]
        public string Message { get; set; }

        private readonly IConfiguration _configuration;
        public UsuarioModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task OnGetAsync()
        {
            var identityApi = GetIIdentityApi();
            var rolesResponse = await identityApi.GetRolesNamesAsync();

            if (rolesResponse.IsSuccessStatusCode)
            {
                Roles = rolesResponse.Content.Select(x => new SelectListItem { Text = x, Value = x }).ToList();
            }

            EditMode = false;
        }

        public async Task<IActionResult> OnPostNewUsuarioAsync()
        {
            var identityApi = GetIIdentityApi();

            if (!ModelState.IsValid)
            {
                Message = "Por favor complete el formulario correctamente";
                var rolesResponse = await identityApi.GetRolesNamesAsync();

                if (rolesResponse.IsSuccessStatusCode)
                {
                    Roles = rolesResponse.Content.Select(x => new SelectListItem { Text = x, Value = x }).ToList();
                }

                EditMode = false;

                return Page();
            }

            var registerResponse = await identityApi.RegisterAsync(new CIAC_TAS_Service.Contracts.V1.Requests.UserRegistrationRequest
            {
                UserName = UsuarioModelView.UserName,
                Email = UsuarioModelView.Email,
                Password = UsuarioModelView.Password
            });

            if (!registerResponse.IsSuccessStatusCode)
            {
                var authFailedResponse = JsonConvert.DeserializeObject<AuthFailedResponse>(registerResponse.Error.Content);

                Message = String.Join(", ", authFailedResponse.Errors);
                var rolesResponse = await identityApi.GetRolesNamesAsync();

                if (rolesResponse.IsSuccessStatusCode)
                {
                    Roles = rolesResponse.Content.Select(x => new SelectListItem { Text = x, Value = x }).ToList();
                }

                EditMode = false;

                return Page();
            }

            var userResponse = await identityApi.GetUserByNameAsync(UsuarioModelView.UserName);

            if (userResponse.IsSuccessStatusCode)
            {
                await identityApi.AsignUserToRoleAsync(new CIAC_TAS_Service.Contracts.V1.Requests.AsignRoleToUserRequest
                {
                    UserId = userResponse.Content.Id,
                    RoleName = UsuarioModelView.Role
                });
            }

            EditMode = false;

            return RedirectToPage("/Usuario/Usuarios");
        }

        public async Task<IActionResult> OnGetEditUsuarioAsync(string userName)
        {
            var identityApi = GetIIdentityApi();
            var userResponse = await identityApi.GetUserByNameAsync(userName);

            if (!userResponse.IsSuccessStatusCode)
            {
                Message = "Ocurrio un error inesperado";

                return RedirectToPage("/Usuario/Usuarios");
            }

            var rolesResponse = await identityApi.GetRolesByUserNameAsync(userName);
            if (!rolesResponse.IsSuccessStatusCode)
            {
                Message = "Ocurrio un error inesperado";

                return RedirectToPage("/Usuario/Usuarios");
            }

            var usuario = userResponse.Content;
            var roles = rolesResponse.Content;

            UsuarioModelView = new UsuarioModelView
            {
                UserName = usuario.UserName,
                Email = usuario.Email,
                Role = roles.FirstOrDefault()
            };

            EditMode = true;

            return Page();
        }

        public async Task<IActionResult> OnPostEditUsuarioAsync(string userName)
        {
            if (UsuarioModelView.Password == null || UsuarioModelView.Password == string.Empty)
            {
                EditMode = true;

                return Page();
            }

            var identityApi = GetIIdentityApi();
            var updatePasswordResponse = await identityApi.PatchUserPasswordAsync(userName, new CIAC_TAS_Service.Contracts.V1.Requests.PatchUsuarioPasswordRequest
            {
                NewPassword = UsuarioModelView.Password
            });

            if (!updatePasswordResponse.IsSuccessStatusCode)
            {
                var authFailedResponse = JsonConvert.DeserializeObject<AuthFailedResponse>(updatePasswordResponse.Error.Content);

                Message = String.Join(", ", authFailedResponse.Errors);
                EditMode = true;

                return Page();
            }

            return RedirectToPage("/Usuario/Usuarios");
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

    }
}
