using System.ComponentModel.DataAnnotations;

namespace CIAC_TAS_Web_UI.ModelViews.General
{
    public class ModuloModelView
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Asigne a un Codigo al Modulo")]
        public string ModuloCodigo { get; set; }
        [Required(ErrorMessage = "Asigne a un Nombre")]
        public string Nombre { get; set; }
    }
}
