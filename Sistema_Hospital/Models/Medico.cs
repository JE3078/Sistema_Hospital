using System;
using System.Collections.Generic;

namespace Sistema_Hospital.Models;

public partial class Medico
{
    public int IdMedico { get; set; }

    public string Nombre { get; set; } = null!;

    public string Apellido { get; set; } = null!;

    public string Dpi { get; set; } = null!;

    public string Colegiado { get; set; } = null!;

    public DateOnly FechaNacimiento { get; set; }

    public string Telefono { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public int IdGenero { get; set; }

    public int IdEspecialidad { get; set; }

    public string? Direccion { get; set; }

    public int? IdUsuario { get; set; }

    public bool Estado {  get; set; }

    public virtual ICollection<Cita> Cita { get; set; } = new List<Cita>();

    public virtual Especialidad IdEspecialidadNavigation { get; set; } = null!;

    public virtual Genero IdGeneroNavigation { get; set; } = null!;

    public virtual UsuarioSistema? IdUsuarioNavigation { get; set; }
}
