using System;
using System.Collections.Generic;

namespace Sistema_Hospital.Models;

public partial class Ingreso
{
    public int IdIngreso { get; set; }

    public string Motivo { get; set; } = null!;

    public int IdPaciente { get; set; }

    public int IdEstado { get; set; }

    public int IdArea { get; set; }

    public DateTime FechaIngreso { get; set; }

    public DateTime? FechaSalida { get; set; }

    public virtual AreaHospital IdAreaNavigation { get; set; } = null!;

    public virtual Estado IdEstadoNavigation { get; set; } = null!;

    public virtual Paciente IdPacienteNavigation { get; set; } = null!;
}
