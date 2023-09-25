using System.ComponentModel.DataAnnotations;

namespace CIAC_TAS_Web_UI.ModelViews.Instructores
{
    public class InstructorGrupoModelView
    {
        [Required]
        public int InstructorId { get; set; }
        public string? InstructorNombre { get; set; }
        [Required]
        public int GrupoId { get; set; }
        public string? GrupoNombre { get; set; }
        [Required]
        public int MateriaId { get; set; }
        public string? MateriaNombre { get; set; }

        public List<int>? InstructoresIds { get; set; } = new List<int>();
    }
}
