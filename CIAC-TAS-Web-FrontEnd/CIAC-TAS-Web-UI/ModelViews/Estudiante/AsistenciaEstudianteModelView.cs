using System.ComponentModel.DataAnnotations;

namespace CIAC_TAS_Web_UI.ModelViews.Estudiante
{
    public class AsistenciaEstudianteModelView
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Asigne a un Estudiante")]
        public int EstudianteId { get; set; }
        public int TipoAsistenciaId { get; set; }

        public string? EstudianteNombre { get; set; }
        public string? TipoAsistenciaNombre { get; set; }
    }
}
