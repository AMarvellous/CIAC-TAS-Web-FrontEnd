using System.ComponentModel.DataAnnotations;

namespace CIAC_TAS_Web_UI.ModelViews.Estudiante
{
    public class EstudianteMateriaModelView
    {
        [Required]
        public int EstudianteId { get; set; }
        public string? EstudianteNombre { get; set; }
        [Required]
        public int GrupoId { get; set; }
        public string? GrupoNombre { get; set; }
        [Required]
        public int MateriaId { get; set; }
        public string? MateriaNombre { get; set; }

        public List<int>? EstudiantesIds { get; set; } = new List<int>();
    }
}
