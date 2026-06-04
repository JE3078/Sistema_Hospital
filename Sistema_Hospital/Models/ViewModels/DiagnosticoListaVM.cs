namespace Sistema_Hospital.Models.ViewModels
{
    public class DiagnosticoListaVM
    {
        public int IdDiagnostico { get; set; }
        public int IdCita { get; set; }
        public string PacienteNombre { get; set; } = null!;
        public string MedicoNombre { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public string Observaciones { get; set; } = null!;
    }
}
