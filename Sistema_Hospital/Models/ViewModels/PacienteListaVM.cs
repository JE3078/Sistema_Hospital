namespace Sistema_Hospital.Models.ViewModels
{
    public class PacienteListaVM
    {
        public int IdPaciente { get; set; }
        public string Nombre { get; set; } = null!;
        public string Apellido { get; set; } = null!;
        public string Dpi { get; set; } = null!;
        public DateOnly FechaNacimiento { get; set; }
        public string Telefono { get; set; } = null!;
        public string Correo { get; set; } = null!;
        public string Genero { get; set; } = null!;
    }
}