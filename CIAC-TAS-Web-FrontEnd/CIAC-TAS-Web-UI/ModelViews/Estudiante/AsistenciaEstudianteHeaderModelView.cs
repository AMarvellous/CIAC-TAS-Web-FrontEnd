using CIAC_TAS_Web_UI.ModelViews.ASA;
using System.ComponentModel.DataAnnotations;

namespace CIAC_TAS_Web_UI.ModelViews.Estudiante
{
    public class AsistenciaEstudianteHeaderModelView
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
        public DateTime Fecha { get; set; }

        public string? ProgramaNombre { get; set; }
        public string? GrupoNombre { get; set; }
        public string? MateriaNombre { get; set; }
        public string? ModuloNombre { get; set; }
        public string? InstructorNombre { get; set; }

        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public int TotalHorasTeoricas { get; set; }
        public int TotalHorasPracticas { get; set; }
        public string Tema { get; set; }

        public List<AsistenciaEstudianteModelView>? AsistenciaEstudianteModelView { get; set; } = new List<AsistenciaEstudianteModelView>();
    }
}
