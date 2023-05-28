using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.Helper;
using CIAC_TAS_Web_UI.ModelViews.Estudiante;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Refit;

namespace CIAC_TAS_Web_UI.Pages.Estudiante
{
    public class EstudiantesGruposModel : PageModel
    {
        [BindProperty]
        public IEnumerable<EstudianteGrupoModelView> EstudianteGrupoModelView { get; set; } = new List<EstudianteGrupoModelView>();
        public int NewGrupoId { get; set; }
		public List<SelectListItem> GrupoOptions { get; set; }

		[TempData]
        public string Message { get; set; }

        private readonly IConfiguration _configuration;
        public EstudiantesGruposModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var estudianteGrupoServiceApi = RestService.For<IEstudianteGrupoServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });

			await FillGrupoOptions();
			var estudianteGrupoResponse = await estudianteGrupoServiceApi.GetAllGrupoHeadersAsync();

            if (!estudianteGrupoResponse.IsSuccessStatusCode)
            {
                Message = "Ocurrio un error inesperado";

                return Page();
            }

            var estudiantesGrupos = estudianteGrupoResponse.Content.Data;
            EstudianteGrupoModelView = estudiantesGrupos.Select(x => new EstudianteGrupoModelView
            {
                GrupoId = x.GrupoId,
                GrupoNombre = x.GrupoResponse.Nombre
            });

            return Page();
        }

		private async Task FillGrupoOptions()
		{
			var grupoServiceApi = GetIGrupoServiceApiServiceApi();
			var grupoResponse = await grupoServiceApi.GetAllNotAssignedEstudentsAsync();

			if (grupoResponse.IsSuccessStatusCode)
			{
				GrupoOptions = grupoResponse.Content.Data.Select(x => new SelectListItem { Text = x.Nombre, Value = x.Id.ToString() }).ToList();
			}
		}

		private IGrupoServiceApi GetIGrupoServiceApiServiceApi()
		{
			return RestService.For<IGrupoServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
			{
				AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
			});
		}
	}
}
