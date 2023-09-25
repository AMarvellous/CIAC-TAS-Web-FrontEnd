using CIAC_TAS_Web_UI.Validations;
using System.ComponentModel.DataAnnotations;

namespace CIAC_TAS_Web_UI.ModelViews.Instructores
{
    public class ProgramaAnaliticoPdfModelView
    {
        public int? Id { get; set; }
        public string? RutaPdf { get; set; }
        [Required(ErrorMessage = "Asigne una Materia")]
        public int MateriaId { get; set; }
        public string? MateriaNombre { get; set; }
        [Required(ErrorMessage = "Ingrese una gestion")]
        [NotZeroAttribute]
        public int Gestion { get; set; }
    }
}
