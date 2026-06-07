using System;

namespace Sistema_Hospital.Models.ViewModels
{
    public class IngresoListaVM
    {
        public int IdIngreso { get; set; }
        public string Motivo { get; set; } = null!;
        public string PacienteCompleto { get; set; } = null!;
        public string AreaHospitalaria { get; set; } = null!;
        public string Estado { get; set; } = null!;
        public DateTime FechaIngreso { get; set; }
        public DateTime? FechaSalida { get; set; }
    }
}