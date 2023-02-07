using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.Helper;
using CIAC_TAS_Web_UI.ModelViews.Usuario;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Refit;

namespace CIAC_TAS_Web_UI.Pages.Usuario
{
    public class UsuariosModel : PageModel
    {
        [BindProperty]
        public IEnumerable<UsuarioModelView> UsuarioModelViews { get; set; } = new List<UsuarioModelView>();

        [TempData]
        public string Message { get; set; }

        private readonly IConfiguration _configuration;
        public UsuariosModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri(_configuration.GetValue<string>("ServiceUrl")),
                DefaultRequestHeaders = {
                        {"Authorization", $"Bearer {HttpContext.Session.GetString(Session.SessionToken)}"}
                    }
            };
            var identityApi = RestService.For<IIdentityApi>(client);
            var usersResponse = await identityApi.GetUsersAsync();

            if (!usersResponse.IsSuccessStatusCode)
            {
                Message = "Ocurrio un error inesperado";

                return Page();
            }

            var userResponses = usersResponse.Content.Data;
            UsuarioModelViews = userResponses.Select(x => new UsuarioModelView
            {
                UserName = x.UserName,
                Email = x.Email
            });

            return Page();
        }
    }
}
