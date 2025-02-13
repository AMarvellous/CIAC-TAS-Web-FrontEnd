using System.ComponentModel.DataAnnotations;

namespace CIAC_TAS_Web_UI.ModelViews.Estudiante
{
    public class InhabilitarEstudianteModelView
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Asigne a un Estudiante")]
        public int EstudianteId { get; set; }
        public string Motivo { get; set; }

        public string? EstudianteNombre { get; set; }
    }
}
