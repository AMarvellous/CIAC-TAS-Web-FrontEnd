using CIAC_TAS_Service.Contracts.V1.Responses;
using CIAC_TAS_Service.Sdk;
using CIAC_TAS_Web_UI.Helper;
using CIAC_TAS_Web_UI.ModelViews.ASA;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Refit;

namespace CIAC_TAS_Web_UI.Pages.ASA
{
    public class CuestionarioASAPracticaModel : PageModel
    {
        [BindProperty]
        public ThumbnailViewModel ThumbnailViewModel { get; set; }

        [TempData]
        public string Message { get; set; }

        private readonly IConfiguration _configuration;

        public CuestionarioASAPracticaModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> OnGetCuestionarioASAPracticaAsync()
        {
            var userId = HttpContext.Session.GetString(Session.SessionUserId);
            var respuestasAsaServiceApi = GetIRespuestasAsaServiceApi();
            var respuestasAsaResponse = await respuestasAsaServiceApi.GetAllByUserIdAsync(userId);

            if (!respuestasAsaResponse.IsSuccessStatusCode)
            {
                Message = "Ocurrio un error";

                return RedirectToPage("/ASA/CuestionarioASA");
            }

            var respuestasAsaResponses = respuestasAsaResponse.Content.Data;
            
            ThumbnailViewModel = new ThumbnailViewModel();
            ThumbnailViewModel.NumeroPreguntas = respuestasAsaResponses.Count();
            ThumbnailViewModel.ThumbnailModelList = new List<ThumbnailModel>();

            List<RespuestasAsaResponse> _detaisllist = new List<RespuestasAsaResponse>();

            foreach (var item in respuestasAsaResponses)
            {
                _detaisllist.Add(item);
            }

            var listOfBatches = _detaisllist.Batch(10);
            int tabNo = 1;

            foreach (var batchItem in listOfBatches)
            {
                // Generating tab
                ThumbnailModel obj = new ThumbnailModel();
                obj.ThumbnailLabel = "Lebel" + tabNo;
                obj.ThumbnailTabId = "tab" + tabNo;
                obj.ThumbnailTabNo = tabNo;
                obj.Thumbnail_Aria_Controls = "tab" + tabNo;
                obj.Thumbnail_Href = "#tab" + tabNo;

                // batch details
                obj.CuestionarioPreguntasAndRespuestasAsaView = new List<RespuestasAsaResponse>();

                foreach (var item in batchItem)
                {
                    RespuestasAsaResponse detailsObj = new RespuestasAsaResponse();
                    detailsObj = item;
                    obj.CuestionarioPreguntasAndRespuestasAsaView.Add(detailsObj);
                }

                ThumbnailViewModel.ThumbnailModelList.Add(obj);

                tabNo++;
            }

            // Getting first tab data
            var first = ThumbnailViewModel.ThumbnailModelList.FirstOrDefault();

            // Getting first tab data
            var last = ThumbnailViewModel.ThumbnailModelList.LastOrDefault();

            foreach (var item in ThumbnailViewModel.ThumbnailModelList)
            {
                if (item.ThumbnailTabNo == first.ThumbnailTabNo)
                {
                    item.Thumbnail_ItemPosition = "first";
                }

                if (item.ThumbnailTabNo == last.ThumbnailTabNo)
                {
                    item.Thumbnail_ItemPosition = "last";
                }
            }


            return Page();
        }

        private IRespuestasAsaServiceApi GetIRespuestasAsaServiceApi()
        {
            return RestService.For<IRespuestasAsaServiceApi>(_configuration.GetValue<string>("ServiceUrl"), new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(HttpContext.Session.GetString(Session.SessionToken))
            });
        }
    }
}
