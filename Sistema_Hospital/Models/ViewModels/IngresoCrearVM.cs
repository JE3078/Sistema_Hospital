using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Sistema_Hospital.Models.ViewModels
{
    public class IngresoCrearVM
    {
        [Required(ErrorMessage = "El motivo del ingreso es obligatorio.")]
        [StringLength(250, ErrorMessage = "El motivo no puede exceder los 250 caracteres.")]
        [Display(Name = "Motivo de Ingreso")]
        public string Motivo { get; set; } = null!;

        [Required(ErrorMessage = "Debe seleccionar un paciente.")]
        [Display(Name = "Paciente")]
        public int IdPaciente { get; set; }

        [Required(ErrorMessage = "Debe asignar un estado inicial.")]
        [Display(Name = "Estado Inicial")]
        public int IdEstado { get; set; }

        [Required(ErrorMessage = "Debe asignar un área hospitalaria.")]
        [Display(Name = "Área de Destino")]
        public int IdArea { get; set; }

        // Listas auxiliares para llenar los comboboxes en el HTML
        public List<SelectListItem>? Pacientes { get; set; }
        public List<SelectListItem>? Estados { get; set; }
        public List<SelectListItem>? Areas { get; set; }
    }
}