using System.ComponentModel.DataAnnotations;

namespace CIAC_TAS_Web_UI.ModelViews.ASA
{
    public class PreguntaAsaOpcionModelView
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Asigne una Opcion")]
        [Display(Name = "Opcion")]
        public int Opcion { get; set; }

        [Required(ErrorMessage = "Asigne un Texto a la opcion")]
        public string Texto { get; set; }

        [Required(ErrorMessage = "Asigne un valor")]
        public bool RespuestaValida { get; set; }

        [Required(ErrorMessage = "Asigne una Pregunta a esta opcion")]
        public int PreguntaAsaId { get; set; }
    }
}
