using Microsoft.AspNetCore.Mvc.Rendering;

namespace CIAC_TAS_Web_UI.Helper
{
    public static class PredefinedListsHelper
    {
        public enum SexoEnum
        {
            Femenino,
            Masculino
        }

        public static class YesNo
        {
            public static readonly List<SelectListItem> YesNoOptions = new List<SelectListItem>()
            {
                new SelectListItem { Text = "No", Value = "False" },
                new SelectListItem { Text = "Si", Value = "True"}              
            };
        }

        public static class NotaEstudiate
        {
            public static readonly List<SelectListItem> NotaEstudianteOptions = new List<SelectListItem>()
            {
                new SelectListItem { Text = "Progreso", Value = "Progreso" },
                new SelectListItem { Text = "Dominio", Value = "Dominio"}
            };
        }
    }
}
