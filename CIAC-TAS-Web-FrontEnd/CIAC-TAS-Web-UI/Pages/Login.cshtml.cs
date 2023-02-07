using CIAC_TAS_Service.Contracts.V1.Requests;
using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.Helper;
using CIAC_TAS_Web_UI.ModelViews.Login;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Refit;

namespace CIAC_TAS_Web_UI.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public LoginModelView LoginModelView { get; set; }
        
        [TempData]
        public string Message { get; set; }

        private readonly IConfiguration _configuration;
        public LoginModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task OnGetAsync()
        {
        }

        public async Task<IActionResult> OnPostLoginAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var identityApi = RestService.For<IIdentityApi>(_configuration.GetValue<string>("ServiceUrl"));           

            try
            {
                var loginResponse = await identityApi.LoginAsync(new UserLoginRequest
                {
                    UserName = LoginModelView.UserName,
                    Password = LoginModelView.Password
                });

                if (!loginResponse.IsSuccessStatusCode)
                {
                    Message = "Usuario y/o Password son incorrectos";

                    return RedirectToPage("Login");
                }

                var token = loginResponse.Content.Token;
                var client = new HttpClient
                {
                    BaseAddress = new Uri(_configuration.GetValue<string>("ServiceUrl")),
                    DefaultRequestHeaders = {
                        {"Authorization", $"Bearer {token}"}
                    }
                };
                var identityApiWithHeader = RestService.For<IIdentityApi>(client);

                var rolesResponse = await identityApiWithHeader.GetRolesByUserNameAsync(LoginModelView.UserName);
                
                var roles = string.Empty;
                
                if (rolesResponse.IsSuccessStatusCode)
                {
                    roles = String.Join(", ", rolesResponse.Content.Select(i => i.ToString()).ToArray());
                }

                var userResponse = await identityApiWithHeader.GetUserByNameAsync(LoginModelView.UserName);
                var userId = string.Empty;
                if (userResponse.IsSuccessStatusCode)
                {
                    userId = userResponse.Content.Id;
                }

                HttpContext.Session.SetString(Session.SessionRoles, roles);
                HttpContext.Session.SetString(Session.SessionUserName, LoginModelView.UserName);
                HttpContext.Session.SetString(Session.SessionToken, loginResponse.Content.Token);
                HttpContext.Session.SetString(Session.SessionUserId, userId);
            }
            catch (Exception ex)
            {
                Message = "Ocurrio un error inesperado";

                return Page();
            }

            return RedirectToPage("Index");
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            return Page();
        }
    }
}
