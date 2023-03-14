using CIAC_TAS_Service.Contracts.V1.Responses;
using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.Helper;
using CIAC_TAS_Web_UI.ModelViews.Usuario;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Refit;
using System.Data;

namespace CIAC_TAS_Web_UI.Pages.Usuario
{
    public class UsuarioProfileModel : PageModel
    {
        [BindProperty]
        public UsuarioProfileModelView UsuarioProfileModelView { get; set; } = new UsuarioProfileModelView();

        [TempData]
        public string Message { get; set; }

        private readonly IConfiguration _configuration;
        public UsuarioProfileModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            //var identityApi = GetIIdentityApi();
            var userName = HttpContext.Session.GetString(Session.SessionUserName);
            //var userResponse = await identityApi.GetUserByNameAsync(userName);

            //if (!userResponse.IsSuccessStatusCode)
            //{
            //    return RedirectToPage("Index");
            //}

            //var user = userResponse.Content;

            UsuarioProfileModelView.UserName = userName;
            UsuarioProfileModelView.Password = string.Empty;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var identityApi = GetIIdentityApi();
            var userName = HttpContext.Session.GetString(Session.SessionUserName);
            var userResponse = await identityApi.PatchUserPasswordUserOwnsForEstudianteOrInstructorAsync(userName, new CIAC_TAS_Service.Contracts.V1.Requests.PatchUsuarioPasswordRequest
            {
                NewPassword = UsuarioProfileModelView.Password
            });

            if (!userResponse.IsSuccessStatusCode)
            {
                var authFailedResponse = JsonConvert.DeserializeObject<AuthFailedResponse>(userResponse.Error.Content);

                Message = String.Join(", ", authFailedResponse.Errors);

                return Page();
            }

            HttpContext.Session.Clear();

            return RedirectToPage("/Login");
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
