using System.ComponentModel.DataAnnotations;

namespace CIAC_TAS_Web_UI.ModelViews.Usuario
{
    public class UsuarioModelView
    {
        [Required(ErrorMessage = "Ingrese un Email")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Ingrese un Nombre de Usuario")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Ingrese un Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Debe asignar un Rol")]
        public string Role { get; set; }
    }
}
