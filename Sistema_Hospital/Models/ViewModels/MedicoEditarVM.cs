using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Sistema_Hospital.Models.ViewModels
{
    public class MedicoEditarVM
    {
        // ID obligatorio para saber a quién estamos editando
        [Required]
        public int IdMedico { get; set; }

        // --- DATOS DEL MÉDICO (Editables) ---
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50)]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(50)]
        public string Apellido { get; set; } = null!;

        [Required(ErrorMessage = "El DPI es obligatorio.")]
        [StringLength(13, MinimumLength = 13, ErrorMessage = "El DPI debe tener 13 dígitos.")]
        public string DPI { get; set; } = null!;

        [Required(ErrorMessage = "El número de colegiado es obligatorio.")]
        [StringLength(50)]
        public string Colegiado { get; set; } = null!;

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
        [DataType(DataType.Date)]
        public DateTime FechaNacimiento { get; set; }

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

        // --- DATOS DE LA CUENTA (Seguridad) ---
        public string Username { get; set; } = null!; // Solo lectura en la Vista

        [StringLength(100, MinimumLength = 6, ErrorMessage = "La nueva contraseña debe tener al menos 6 caracteres.")]
        public string? NuevaPassword { get; set; } // Opcional

        // --- LISTAS PARA DROPDOWNS ---
        public List<SelectListItem>? Generos { get; set; }
        public List<SelectListItem>? Especialidades { get; set; }
    }
}