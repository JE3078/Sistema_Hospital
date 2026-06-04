using System.ComponentModel.DataAnnotations;

namespace Sistema_Hospital.Models.ViewModels
{
    public class DiagnosticoEditarVM
    {
        [Required]
        public int IdDiagnostico { get; set; }

        [Required]
        public int IdCita { get; set; }

        public string? PacienteNombre { get; set; }

        [Required(ErrorMessage = "La descripción del diagnóstico es obligatoria.")]
        [StringLength(250, ErrorMessage = "La descripción no puede exceder los 250 caracteres.")]
        [Display(Name = "Descripción del Diagnóstico")]
        public string Descripcion { get; set; } = null!;

        [Required(ErrorMessage = "Las observaciones son obligatorias.")]
        [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder los 500 caracteres.")]
        [Display(Name = "Observaciones Médicas")]
        public string Observaciones { get; set; } = null!;
    }
}