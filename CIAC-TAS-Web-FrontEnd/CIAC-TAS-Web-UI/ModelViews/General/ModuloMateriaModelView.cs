using System.ComponentModel.DataAnnotations;

namespace CIAC_TAS_Web_UI.ModelViews.General
{
    public class ModuloMateriaModelView
    {
        [Required(ErrorMessage = "Asigne a un Modulo")]
        public int ModuloId { get; set; }
        public ModuloModelView? ModuloModelView { get; set; }
        [Required(ErrorMessage = "Asigne a una Materia")]
        public int MateriaId { get; set; }
        public MateriaModelView? MateriaModelView { get; set; }
    }
}
