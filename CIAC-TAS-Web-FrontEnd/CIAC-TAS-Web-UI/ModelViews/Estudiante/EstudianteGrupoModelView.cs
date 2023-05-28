using System.ComponentModel.DataAnnotations;

namespace CIAC_TAS_Web_UI.ModelViews.Estudiante
{
    public class EstudianteGrupoModelView
    {
        [Required]
        public int EstudianteId { get; set; }
        public string? EstudianteNombre { get; set; }
        [Required]
        public int GrupoId { get; set; }
        public string? GrupoNombre { get; set; }

		public List<int>? EstudiantesIds { get; set; } = new List<int>();
	}
}
