using System;
using System.Collections.Generic;

namespace Sistema_Hospital.Models;

public partial class AreaHospital
{
    public int IdArea { get; set; }

    public string NombreArea { get; set; } = null!;

    public string? Descripcion { get; set; }

    public virtual ICollection<Ingreso> Ingresos { get; set; } = new List<Ingreso>();
}
