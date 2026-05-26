using System;
using System.Collections.Generic;

namespace Sistema_Hospital.Models;

public partial class Estado
{
    public int IdEstado { get; set; }

    public string NombreEstado { get; set; } = null!;

    public string? Descripcion { get; set; }

    public virtual ICollection<Cita> Cita { get; set; } = new List<Cita>();

    public virtual ICollection<Ingreso> Ingresos { get; set; } = new List<Ingreso>();
}
