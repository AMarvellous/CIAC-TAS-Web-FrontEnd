using System.ComponentModel.DataAnnotations;

namespace CIAC_TAS_Web_UI.ModelViews.Login
{
    public class LoginModelView
    {
        [Required(ErrorMessage = "Ingrese un Usuario")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Ingrese un Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
