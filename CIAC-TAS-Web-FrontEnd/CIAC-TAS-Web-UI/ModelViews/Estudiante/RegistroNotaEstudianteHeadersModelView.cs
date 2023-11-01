using System.ComponentModel.DataAnnotations;

namespace CIAC_TAS_Web_UI.ModelViews.Estudiante
{
    public class RegistroNotaEstudianteHeadersModelView
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Asigne un estudiante")]
        public int EstudianteId { get; set; }

        public string? EstudianteNombre { get; set; }
    }
}
