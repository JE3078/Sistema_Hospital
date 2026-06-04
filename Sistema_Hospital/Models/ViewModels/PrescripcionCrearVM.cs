using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Sistema_Hospital.Models.ViewModels
{
    public class PrescripcionCrearVM
    {
        public int IdDiagnostico { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un medicamento.")]
        [Display(Name = "Medicamento")]
        public int IdMedicamento { get; set; }

        [Required(ErrorMessage = "La dosis es obligatoria (Ej. 500mg, 1 Tableta).")]
        [StringLength(50)]
        public string Dosis { get; set; } = null!;

        [Required(ErrorMessage = "La frecuencia es obligatoria (Ej. Cada 8 horas).")]
        [StringLength(50)]
        public string Frecuencia { get; set; } = null!;

        [Required(ErrorMessage = "La duración es obligatoria (Ej. 7 días).")]
        [StringLength(50)]
        public string Duracion { get; set; } = null!;

        [StringLength(250, ErrorMessage = "Las instrucciones no pueden exceder los 250 caracteres.")]
        [Display(Name = "Instrucciones Adicionales")]
        public string? Instrucciones { get; set; }

        // Lista para llenar el DropDownList de medicamentos
        public List<SelectListItem>? Medicamentos { get; set; }
    }
}
