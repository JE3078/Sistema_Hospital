namespace Sistema_Hospital.Models.ViewModels
{
    public class CitaListaVM
    {
        public int IdCita { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public DateTime FechaHora { get; set; }
        public string NombreMedico { get; set; } = string.Empty;
        public string NombrePaciente { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }
}
