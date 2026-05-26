using System;
using System.Collections.Generic;

namespace Sistema_Hospital.Models;

public partial class Prescripcion
{
    public int IdPrescripcion { get; set; }

    public int IdDiagnostico { get; set; }

    public int IdMedicamento { get; set; }

    public string Dosis { get; set; } = null!;

    public string Frecuencia { get; set; } = null!;

    public string Duracion { get; set; } = null!;

    public string? Instrucciones { get; set; }

    public virtual Diagnostico IdDiagnosticoNavigation { get; set; } = null!;

    public virtual Medicamento IdMedicamentoNavigation { get; set; } = null!;
}
