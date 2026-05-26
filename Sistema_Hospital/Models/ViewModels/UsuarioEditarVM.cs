using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Sistema_Hospital.Models.ViewModels
{
    public class UsuarioEditarVM
    {
        [Required]
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre de usuario no puede exceder los 50 caracteres.")]
        public string Username { get; set; } = null!;

        [DataType(DataType.Password)]
        public string? NuevoPassword { get; set; } // Opcional

        [Required(ErrorMessage = "Debe asignar un rol.")]
        public int IdRol { get; set; }

        [Required(ErrorMessage = "El estado es requerido.")]
        public bool Estado { get; set; }

        public List<SelectListItem>? Roles { get; set; }
    }
}
