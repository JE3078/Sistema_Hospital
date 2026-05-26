using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Sistema_Hospital.Models.ViewModels
{
    public class MedicoCrearVM
    {
        // --- CUENTA DE USUARIO SISTEMA ---
        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        [StringLength(50)]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mínimo 6 caracteres.")]
        public string Password { get; set; } = null!;

        // --- DATOS DEL MÉDICO ---
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50)]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(50)]
        public string Apellido { get; set; } = null!;

        [Required(ErrorMessage = "El DPI es obligatorio.")]
        [StringLength(13, MinimumLength = 13, ErrorMessage = "El DPI debe tener exactamente 13 dígitos.")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "El DPI solo puede contener números.")]
        public string DPI { get; set; } = null!;

        [Required(ErrorMessage = "El número de colegiado es obligatorio.")]
        [StringLength(50)]
        public string Colegiado { get; set; } = null!;

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
        [DataType(DataType.Date)]
        public DateTime FechaNacimiento { get; set; } = DateTime.Today.AddYears(-25); // Propuesta razonable de inicio

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [StringLength(8, MinimumLength = 8, ErrorMessage = "El teléfono debe tener 8 dígitos.")]
        public string Telefono { get; set; } = null!;

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
        [StringLength(100)]
        public string Correo { get; set; } = null!;

        [Required(ErrorMessage = "Debe seleccionar el género.")]
        public int IdGenero { get; set; }

        [Required(ErrorMessage = "Debe seleccionar la especialidad.")]
        public int IdEspecialidad { get; set; }

        [StringLength(100)]
        public string? Direccion { get; set; }

        // --- LISTAS PARA DROPDOWNS ---
        public List<SelectListItem>? Generos { get; set; }
        public List<SelectListItem>? Especialidades { get; set; }
    }
}
