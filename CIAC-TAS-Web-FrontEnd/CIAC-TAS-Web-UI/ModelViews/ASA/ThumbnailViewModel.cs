namespace CIAC_TAS_Web_UI.ModelViews.ASA
{
    public class ThumbnailViewModel
    {
        public int NumeroPreguntas { get; set; }
        public int RespuestasRespondidas { get; set; }
		public int RespuestasNoRespondidas { get; set; }
		public int RespuestasNoSeguras { get; set; }        
        public long TiempoRestante { get; set; }
        public List<ThumbnailModel> ThumbnailModelList { get; set; }
    }
}
