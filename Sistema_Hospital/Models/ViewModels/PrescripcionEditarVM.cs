using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Sistema_Hospital.Models.ViewModels
{
    public class PrescripcionEditarVM
    {
        [Required]
        public int IdPrescripcion { get; set; }

        [Required]
        public int IdDiagnostico { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un medicamento.")]
        [Display(Name = "Medicamento")]
        public int IdMedicamento { get; set; }

        [Required(ErrorMessage = "La dosis es obligatoria.")]
        [StringLength(50)]
        public string Dosis { get; set; } = null!;

        [Required(ErrorMessage = "La frecuencia es obligatoria.")]
        [StringLength(50)]
        public string Frecuencia { get; set; } = null!;

        [Required(ErrorMessage = "La duración es obligatoria.")]
        [StringLength(50)]
        public string Duracion { get; set; } = null!;

        [StringLength(250)]
        [Display(Name = "Instrucciones Adicionales")]
        public string? Instrucciones { get; set; }

        public List<SelectListItem>? Medicamentos { get; set; }
    }
}
