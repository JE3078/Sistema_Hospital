using System;
using System.Collections.Generic;

namespace Sistema_Hospital.Models;

public partial class UsuarioSistema
{
    public int IdUsuario { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public int IdRol { get; set; }

    public bool? Estado { get; set; }

    public virtual ICollection<Bitacora> Bitacoras { get; set; } = new List<Bitacora>();

    public virtual Enfermero? Enfermero { get; set; }

    public virtual Rol IdRolNavigation { get; set; } = null!;

    public virtual Medico? Medico { get; set; }
}
