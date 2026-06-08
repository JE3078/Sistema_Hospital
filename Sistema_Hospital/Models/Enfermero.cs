using System;
using System.Collections.Generic;

namespace Sistema_Hospital.Models;

public partial class Enfermero
{
    public int IdEnfermero { get; set; }

    public string Nombre { get; set; } = null!;

    public string Apellido { get; set; } = null!;

    public string Dpi { get; set; } = null!;

    public string Telefono { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public int IdGenero { get; set; }

    public int? IdUsuario { get; set; }

    public bool Estado { get; set; } = true;

    public virtual Genero IdGeneroNavigation { get; set; } = null!;

    public virtual UsuarioSistema? IdUsuarioNavigation { get; set; }
}
