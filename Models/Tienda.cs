using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Tienda
{
    public int TiendaId { get; set; }

    public string Nombre { get; set; } = null!;

    public string Direccion { get; set; } = null!;

    /// <summary>
    /// 1=activa, 0=inactiva
    /// </summary>
    public bool? Estado { get; set; }

    public virtual ICollection<CorteCaja> CorteCaja { get; set; } = new List<CorteCaja>();

    public virtual ICollection<Inventario> Inventario { get; set; } = new List<Inventario>();

    public virtual ICollection<Usuario> Usuario { get; set; } = new List<Usuario>();
}
