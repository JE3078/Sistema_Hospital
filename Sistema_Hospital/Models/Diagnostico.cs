using System;
using System.Collections.Generic;

namespace Sistema_Hospital.Models;

public partial class Diagnostico
{
    public int IdDiagnostico { get; set; }

    public int IdCita { get; set; }

    public string Descripcion { get; set; } = null!;

    public string Observaciones { get; set; } = null!;

    public virtual Cita IdCitaNavigation { get; set; } = null!;

    public virtual ICollection<Prescripcion> Prescripcions { get; set; } = new List<Prescripcion>();
}
