using System;
using System.Collections.Generic;

namespace Sistema_Hospital.Models;

public partial class Bitacora
{
    public int IdLog { get; set; }

    public int IdUsuario { get; set; }

    public string Accion { get; set; } = null!;

    public DateTime FechaHora { get; set; }

    public virtual UsuarioSistema IdUsuarioNavigation { get; set; } = null!;
}
