namespace Sistema_Hospital.Models.ViewModels
{
    public class PrescripcionListaVM
    {
        public int IdPrescripcion { get; set; }
        public int IdDiagnostico { get; set; }
        public string MedicamentoNombre { get; set; } = null!;
        public string Dosis { get; set; } = null!;
        public string Frecuencia { get; set; } = null!;
        public string Duracion { get; set; } = null!;
        public string? Instrucciones { get; set; }
    }
}
