using System;
using System.Collections.Generic;

namespace Sistema_Hospital.Models;

public partial class Genero
{
    public int IdGenero { get; set; }

    public string Descripcion { get; set; } = null!;

    public virtual ICollection<Enfermero> Enfermeros { get; set; } = new List<Enfermero>();

    public virtual ICollection<Medico> Medicos { get; set; } = new List<Medico>();

    public virtual ICollection<Paciente> Pacientes { get; set; } = new List<Paciente>();
}
