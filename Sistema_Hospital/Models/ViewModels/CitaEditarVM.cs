using Microsoft.AspNetCore.Mvc.Rendering;

namespace Sistema_Hospital.Models.ViewModels
{
    public class CitaEditarVM
    {
        public int IdCita { get; set; }

        public int IdPaciente { get; set; }
        public List<SelectListItem>? Pacientes { get; set; }

        public int IdMedico { get; set; }
        public List<SelectListItem>? Medicos { get; set; }

        public int IdEstado { get; set; }
        public List<SelectListItem>? Estados { get; set; }

        public DateTime FechaHora { get; set; }
        public string Motivo { get; set; } = string.Empty;

        public DiagnosticoEditarVM? Diagnostico { get; set; }
        public PrescripcionEditarVM? Prescripcion { get; set; } 
    }
}
