﻿using System.ComponentModel.DataAnnotations;

namespace CIAC_TAS_Web_UI.ModelViews.Instructores
{
	public class InstructorModelView
	{
        public int Id { get; set; }
        [Required(ErrorMessage = "Seleccione un usuario para el Instructor")]
        public string UserId { get; set; }
        [Required(ErrorMessage = "Ingrese el numero de licencia")]
        public string NumeroLicencia { get; set; }
        public DateTime Fecha { get; set; }
        [Required(ErrorMessage = "Ingrese un codigo TAS")]
        public string CodigoTas { get; set; }
        [Required(ErrorMessage = "Ingrese el nombre del Instructor")]
        public string Nombres { get; set; }
        public string? ApellidoPaterno { get; set; }
        public string? ApellidoMaterno { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string? Nacionalidad { get; set; }
        public string? EstadoCivil { get; set; }
        public string? Domicilio { get; set; }
        public string? Telefono { get; set; }
        public string? Celular { get; set; }
        [Required(ErrorMessage = "Ingrese un email")]
        public string Email { get; set; }
        public string? Formacion { get; set; }
        public string? Cursos { get; set; }
        public string? ExperienciaLaboral { get; set; }
        public string? ExperienciaInstruccion { get; set; }
        [Required(ErrorMessage = "Seleccione la vacuna antitetanica")]
        public bool VacunaAntitetanica { get; set; }
    }
}
