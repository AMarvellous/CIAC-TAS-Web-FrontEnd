using CIAC_TAS_Service.Contracts.V1.Responses;

namespace CIAC_TAS_Web_UI.ModelViews.ASA
{
    public class ThumbnailModel
    {
        public string ThumbnailLabel { get; set; }
        public string ThumbnailTabId { get; set; }
        public int ThumbnailTabNo { get; set; }
        public string Thumbnail_Aria_Controls { get; set; }
        public string Thumbnail_Href { get; set; }
        public string Thumbnail_ItemPosition { get; set; }

        public List<RespuestasAsaResponse> CuestionarioPreguntasAndRespuestasAsaView { get; set; }
    }
}
