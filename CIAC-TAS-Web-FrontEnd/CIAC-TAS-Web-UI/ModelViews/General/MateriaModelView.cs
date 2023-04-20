using System.ComponentModel.DataAnnotations;

namespace CIAC_TAS_Web_UI.ModelViews.General
{
    public class MateriaModelView
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Asigne a un Codigo de Materia")]
        public string MateriaCodigo { get; set; }
        [Required(ErrorMessage = "Asigne a un Nombre")]
        public string Nombre { get; set; }
    }
}
