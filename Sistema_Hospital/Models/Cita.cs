using System;
using System.Collections.Generic;

namespace Sistema_Hospital.Models;

public partial class Cita
{
    public int IdCita { get; set; }

    public string Motivo { get; set; } = null!;

    public DateTime FechaHora { get; set; }

    public int IdMedico { get; set; }

    public int IdPaciente { get; set; }

    public int IdEstado { get; set; }

    public virtual ICollection<Diagnostico> Diagnosticos { get; set; } = new List<Diagnostico>();

    public virtual Estado IdEstadoNavigation { get; set; } = null!;

    public virtual Medico IdMedicoNavigation { get; set; } = null!;

    public virtual Paciente IdPacienteNavigation { get; set; } = null!;
}
