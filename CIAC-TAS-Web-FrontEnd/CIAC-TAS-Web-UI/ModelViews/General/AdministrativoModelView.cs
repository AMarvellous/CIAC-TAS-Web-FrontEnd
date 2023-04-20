using System.ComponentModel.DataAnnotations;

namespace CIAC_TAS_Web_UI.ModelViews.General
{
    public class AdministrativoModelView
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Seleccione un usuario para el Administrativo")]
        public string UserId { get; set; }
        [Required(ErrorMessage = "Seleccione una licencia/carnet")]
        public string LicenciaCarnetIdentidad { get; set; }
        public DateTime Fecha { get; set; }
        [Required(ErrorMessage = "Seleccione un nombre")]
        public string Nombres { get; set; }
        public string? ApellidoPaterno { get; set; }
        public string? ApellidoMaterno { get; set; }
        public string? LugarNacimiento { get; set; }
        public string? Sexo { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string? Nacionalidad { get; set; }
        public string? EstadoCivil { get; set; }
        public string? Domicilio { get; set; }
        public string? Telefono { get; set; }
        public string? Celular { get; set; }
        [Required(ErrorMessage = "Seleccione un email")]
        public string Email { get; set; }
        public string? Formacion { get; set; }
        public string? Cursos { get; set; }
        public string? ExperienciaLaboral { get; set; }
        public string? ExperienciaInstruccion { get; set; }
        public bool VacunaAntitetanica { get; set; }
    }
}
