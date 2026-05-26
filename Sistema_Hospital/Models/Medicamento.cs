using System;
using System.Collections.Generic;

namespace Sistema_Hospital.Models;

public partial class Medicamento
{
    public int IdMedicamento { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public int Stock { get; set; }

    public decimal Precio { get; set; }

    public virtual ICollection<Prescripcion> Prescripcions { get; set; } = new List<Prescripcion>();
}
