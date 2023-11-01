using System.ComponentModel.DataAnnotations;

namespace CIAC_TAS_Web_UI.ModelViews.Instructores
{
    public class RegistroNotaEstudianteHeaderModelView
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Asigne a un Estudiante")]
        public int EstudianteId { get; set; }
        public int RegistroNotaHeaderId { get; set; }

        public string? EstudianteNombre { get; set; }
    }
}
