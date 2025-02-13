using CIAC_TAS_Web_UI.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CIAC_TAS_Web_UI.Pages
{
    public class IndexModel : PageModel
    {
        public string MensajeBloqueo { get; set; }
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            MensajeBloqueo = HttpContext.Session.GetString(Session.SessionMotivoBloqueo);
        }
    }
}