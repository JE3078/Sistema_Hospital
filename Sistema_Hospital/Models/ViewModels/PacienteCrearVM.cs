using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Sistema_Hospital.Models.ViewModels
{
    public class PacienteCrearVM
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres.")]
        [Display(Name = "Nombres")]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(50, ErrorMessage = "El apellido no puede exceder los 50 caracteres.")]
        [Display(Name = "Apellidos")]
        public string Apellido { get; set; } = null!;

        [Required(ErrorMessage = "El DPI es obligatorio.")]
        [StringLength(13, MinimumLength = 13, ErrorMessage = "El DPI debe tener exactamente 13 dígitos.")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "El DPI solo debe contener números.")]
        [Display(Name = "DPI (CUI)")]
        public string Dpi { get; set; } = null!;

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Nacimiento")]
        public DateOnly FechaNacimiento { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [StringLength(8, MinimumLength = 8, ErrorMessage = "El teléfono debe tener 8 dígitos.")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "El teléfono solo debe contener números.")]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; } = null!;

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo no es válido.")]
        [StringLength(100, ErrorMessage = "El correo no puede exceder los 100 caracteres.")]
        [Display(Name = "Correo Electrónico")]
        public string Correo { get; set; } = null!;

        [Required(ErrorMessage = "Debe seleccionar un género.")]
        [Display(Name = "Género")]
        public int IdGenero { get; set; }

        [StringLength(100, ErrorMessage = "La dirección no puede exceder los 100 caracteres.")]
        [Display(Name = "Dirección de Residencia")]
        public string? Direccion { get; set; }

        // Propiedad auxiliar para cargar el Select HTML
        public List<SelectListItem>? Generos { get; set; }
    }
}