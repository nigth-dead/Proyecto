using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Usuario
{
    public int UsuarioId { get; set; }

    public int TiendaId { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Telefono { get; set; }

    /// <summary>
    /// Guardar como hash (bcrypt/argon2)
    /// </summary>
    public string Contrasena { get; set; } = null!;

    public string Rol { get; set; } = null!;

    public bool? Trabajando { get; set; }

    public bool? Contratado { get; set; }

    public virtual ICollection<CorteCaja> CorteCaja { get; set; } = new List<CorteCaja>();

    public virtual ICollection<HistorialMovimiento> HistorialMovimiento { get; set; } = new List<HistorialMovimiento>();

    public virtual ICollection<HistorialPedido> HistorialPedido { get; set; } = new List<HistorialPedido>();

    public virtual Tienda Tienda { get; set; } = null!;

    public virtual ICollection<Venta> Venta { get; set; } = new List<Venta>();
}
