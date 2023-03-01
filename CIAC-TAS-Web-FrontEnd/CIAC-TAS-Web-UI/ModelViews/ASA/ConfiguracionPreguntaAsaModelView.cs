using CIAC_TAS_Web_UI.ModelViews.General;

namespace CIAC_TAS_Web_UI.ModelViews.ASA
{
    public class ConfiguracionPreguntaAsaModelView
    {
        public int Id { get; set; }
        public int GrupoId { get; set; }
        public int? CantidadPreguntas { get; set; }
        public DateTime FechaInicial { get; set; }
        public DateTime? FechaFin { get; set; }

        public GrupoModelView? GrupoModelView { get; set; }
    }
}
