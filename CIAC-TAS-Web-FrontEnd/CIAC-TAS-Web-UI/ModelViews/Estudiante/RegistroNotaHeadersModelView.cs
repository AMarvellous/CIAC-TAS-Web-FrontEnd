using System.ComponentModel.DataAnnotations;

namespace CIAC_TAS_Web_UI.ModelViews.Estudiante
{
    public class RegistroNotaHeadersModelView
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Asigne a un Programa")]
        public int ProgramaId { get; set; }

        [Required(ErrorMessage = "Asigne a un Grupo")]
        public int GrupoId { get; set; }

        [Required(ErrorMessage = "Asigne a una Materia")]
        public int MateriaId { get; set; }

        [Required(ErrorMessage = "Asigne a un Modulo")]
        public int ModuloId { get; set; }

        [Required(ErrorMessage = "Asigne a un Instructor")]
        public int InstructorId { get; set; }
        public int PorcentajeDominioTotal { get; set; }
        public int PorcentajeProgresoTotal { get; set; }
        public int TipoRegistroNotaHeaderId { get; set; }

        public string? ProgramaNombre { get; set; }
        public string? GrupoNombre { get; set; }
        public string? MateriaNombre { get; set; }
        public string? ModuloNombre { get; set; }
        public string? InstructorNombre { get; set; }
        public string? TipoRegistroNotaHeaderNombre { get; set; }
    }
}
