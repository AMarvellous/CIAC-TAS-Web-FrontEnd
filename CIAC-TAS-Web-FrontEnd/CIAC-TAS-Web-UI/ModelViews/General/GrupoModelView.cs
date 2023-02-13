using System.ComponentModel.DataAnnotations;

namespace CIAC_TAS_Web_UI.ModelViews.General
{
	public class GrupoModelView
	{
		public int Id { get; set; }

		[Required(ErrorMessage = "Asigne a un Nombre")]
		public string Nombre { get; set; }
	}
}
