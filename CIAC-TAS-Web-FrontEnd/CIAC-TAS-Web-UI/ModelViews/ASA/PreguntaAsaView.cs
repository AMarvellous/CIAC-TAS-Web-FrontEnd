using System.ComponentModel.DataAnnotations;

namespace CIAC_TAS_Web_UI.ModelViews.ASA
{
    public class PreguntaAsaView
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Asigne un Numero a la Pregunta")]
        [Display(Name = "Numero de Pregunta")]
        public int NumeroPregunta { get; set; }

        [Required(ErrorMessage = "Asigne un Texto a la pregunta")]
        public string Pregunta { get; set; }

        public string? Ruta { get; set; }

        [Required(ErrorMessage = "Asigne un Grupo")]
        public int GrupoPreguntaAsaId { get; set; }

        [Required(ErrorMessage = "Asigne un Estado")]
        public int EstadoPreguntaAsaId { get; set; }

        public List<PreguntaAsaOpcionModelView>? PreguntaAsaOpcionesModelViews { get; set; } = new List<PreguntaAsaOpcionModelView>();
    }
}
