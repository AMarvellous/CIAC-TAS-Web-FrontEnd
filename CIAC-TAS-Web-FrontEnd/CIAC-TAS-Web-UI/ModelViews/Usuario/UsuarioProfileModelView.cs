using System.ComponentModel.DataAnnotations;

namespace CIAC_TAS_Web_UI.ModelViews.Usuario
{
    public class UsuarioProfileModelView
    {
        public string? UserName { get; set; }

        [Required(ErrorMessage = "Ingrese un Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
