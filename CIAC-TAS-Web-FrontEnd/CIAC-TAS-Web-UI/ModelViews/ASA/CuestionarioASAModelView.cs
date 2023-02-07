namespace CIAC_TAS_Web_UI.ModelViews.ASA
{
    public class CuestionarioASAModelView
    {
        public int? NumeroPreguntas { get; set; }
        public int? PreguntaIni { get; set; }
        public int? PreguntaFin { get; set; }
        public List<int>? GrupoPreguntaAsaIds { get; set; } = new List<int>();
        public bool? HasQuizInProgress { get; set; }
    }
}
