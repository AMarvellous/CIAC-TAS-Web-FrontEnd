using CIAC_TAS_Web_UI.ModelViews.General;
using System.ComponentModel.DataAnnotations;

namespace CIAC_TAS_Web_UI.ModelViews.ASA
{
    public class ConfiguracionPreguntaAsaModelView
    {
        public int Id { get; set; }
        public int GrupoId { get; set; }
        public int? CantidadPreguntas { get; set; }
        //[DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime FechaInicial { get; set; }
        public DateTime? FechaFin { get; set; }

        public GrupoModelView? GrupoModelView { get; set; }
    }
}
