using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CIAC_TAS_Web_UI.Data;
using CIAC_TAS_Web_UI.ModelViews.ASA;
using CIAC_TAS_Service.Sdk;
using Refit;
using CIAC_TAS_Web_UI.Helper;

namespace CIAC_TAS_Web_UI.Pages.ASA
{
    public class PreguntasAsaModel : PageModel
    {
        [BindProperty]
        public List<PreguntaAsaView> PreguntaAsaView { get; set; } = new List<PreguntaAsaView>();

        [TempData]
        public string Message { get; set; }

        private readonly IConfiguration _configuration;

        public PreguntasAsaModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var preguntaAsaServiceApi = RestService.For<IPreguntaAsaServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
            var preguntaAsaResponse = await preguntaAsaServiceApi.GetAllAsync();

            if (!preguntaAsaResponse.IsSuccessStatusCode)
            {
                Message = "Ocurrio un error inesperado";

                return Page();
            }

            var preguntas = preguntaAsaResponse.Content.Data;
            PreguntaAsaView = preguntas.Select(x => new PreguntaAsaView
            {
                Id = x.Id,
                NumeroPregunta = x.NumeroPregunta,
                Pregunta = x.Pregunta
            }).ToList();

            return Page();
        }
    }
}
