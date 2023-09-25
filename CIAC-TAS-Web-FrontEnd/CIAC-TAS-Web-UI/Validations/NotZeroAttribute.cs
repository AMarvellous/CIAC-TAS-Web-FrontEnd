using System.ComponentModel.DataAnnotations;

namespace CIAC_TAS_Web_UI.Validations
{
    public class NotZeroAttribute : ValidationAttribute
    {
        public override bool IsValid(object value) => (int)value != 0;
    }
}
