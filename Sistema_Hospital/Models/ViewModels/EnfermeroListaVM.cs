using System.ComponentModel.DataAnnotations;

namespace Sistema_Hospital.Models.ViewModels
{
    public class EnfermeroListaVM
    {
        public int IdEnfermero { get; set; }

        public string Nombre { get; set; } = null!;

        public string Apellido { get; set; } = null!;

        public string Telefono { get; set; } = null!;

        public string Correo { get; set; } = null!;

        [Display(Name = "Género")]
        public string Genero { get; set; } = null!;
    }
}
