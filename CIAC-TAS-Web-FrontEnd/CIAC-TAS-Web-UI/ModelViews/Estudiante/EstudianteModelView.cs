using System.ComponentModel.DataAnnotations;

namespace CIAC_TAS_Web_UI.ModelViews.Estudiante
{
    public class EstudianteModelView
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Asigne a un Usuario")]
        public string UserId { get; set; }

        [Required(ErrorMessage = "Ingrese un CI")]
        public string CarnetIdentidad { get; set; }

        [Required(ErrorMessage = "Ingrese un Codigo TAS")]
        public string CodigoTas { get; set; }

        public DateTime? Fecha { get; set; }

        [Required(ErrorMessage = "Ingrese un Nombre")]
        public string Nombre { get; set; }
        public string? ApellidoPaterno { get; set; }
        public string? ApellidoMaterno { get; set; }
        public string? LugarNacimiento { get; set; }
        public string? Sexo { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string? Nacionalidad { get; set; }
        public string? EstadoCivil { get; set; }
        public string? Domicilio { get; set; }
        public string? Telefono { get; set; }
        public string? Celular { get; set; }
        public string? FamiliarTutor { get; set; }

        [Required(ErrorMessage = "Ingrese un Email")]
        public string Email { get; set; }
        public string? NombrePadre { get; set; }
        public string? CelularPadre { get; set; }
        public string? NombreMadre { get; set; }
        public string? CelularMadre { get; set; }
        public string? NombreTutor { get; set; }
        public string? CelularTutor { get; set; }

        [Required(ErrorMessage = "Elija una opcion")]
        public bool VacunaAntitetanica { get; set; }

        [Required(ErrorMessage = "Elija una opcion")]
        public bool ExamenPsicofisiologico { get; set; }
        public string? CodigoSeguro { get; set; }

        public DateTime? FechaSeguro { get; set; }

        [Required(ErrorMessage = "Elija una opcion")]
        public bool InstruccionPrevia { get; set; }

        [Required(ErrorMessage = "Elija una opcion")]
        public bool ExperienciaPrevia { get; set; }
    }
}
