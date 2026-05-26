using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Sistema_Hospital.Models.ViewModels
{
    public class EnfermeroEditarVM
    {
        [Required]
        public int IdEnfermero { get; set; }

        // --- Datos de la Ficha Clínica ---
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres.")]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(50, ErrorMessage = "El apellido no puede exceder los 50 caracteres.")]
        public string Apellido { get; set; } = null!;

        [Required(ErrorMessage = "El DPI es obligatorio.")]
        [StringLength(13, MinimumLength = 13, ErrorMessage = "El DPI debe tener exactamente 13 dígitos.")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "El DPI solo puede contener números.")]
        public string DPI { get; set; } = null!;

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [StringLength(8, MinimumLength = 8, ErrorMessage = "El teléfono debe tener exactamente 8 dígitos.")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "El teléfono solo puede contener números.")]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; } = null!;

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
        [StringLength(100, ErrorMessage = "El correo no puede exceder los 100 caracteres.")]
        [Display(Name = "Correo Electrónico")]
        public string Correo { get; set; } = null!;

        [Required(ErrorMessage = "Debe seleccionar un género.")]
        [Display(Name = "Género")]
        public int IdGenero { get; set; }


        // --- Datos de Gestión Informativa / Credenciales ---
        [Display(Name = "Cuenta de Usuario")]
        public string? Username { get; set; } // Solo informativo en la vista (puedes ponerlo en un campo deshabilitado)

        [StringLength(100, MinimumLength = 6, ErrorMessage = "La nueva contraseña debe tener al menos 6 caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva Contraseña (Dejar en blanco para no cambiar)")]
        public string? NuevaPassword { get; set; }


        // --- Propiedad para rellenar el DropdownList ---
        public List<SelectListItem>? Generos { get; set; }
    }
}