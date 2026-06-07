using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema_Hospital.Models.ViewModels
{
    public class MedicoListaVM
    {
        public int IdMedico { get; set; }
        public string Nombre { get; set; } = null!;
        public string Apellido { get; set; } = null!;
        public string DPI { get; set; } = null!;
        public string Colegiado { get; set; } = null!;
        public string Especialidad { get; set; } = null!;

        public string NombreCompleto => $"{Nombre} {Apellido}";
    }
}
