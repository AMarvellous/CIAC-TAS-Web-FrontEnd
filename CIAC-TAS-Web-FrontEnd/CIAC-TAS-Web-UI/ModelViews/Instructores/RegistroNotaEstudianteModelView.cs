using System.ComponentModel.DataAnnotations;

namespace CIAC_TAS_Web_UI.ModelViews.Instructores
{
    public class RegistroNotaEstudianteModelView
    {
        public int Id { get; set; }
        public int RegistroNotaEstudianteHeaderId { get; set; }
        public double Nota { get; set; }
        public bool TipoDominio { get; set; }
        public string TipoNotaTexto { get; set; }
    }
}
