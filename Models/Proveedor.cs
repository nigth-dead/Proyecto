using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Proveedor
{
    public int ProveedorId { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Telefono { get; set; }

    public string? Correo { get; set; }

    public bool? Activo { get; set; }

    public virtual ICollection<HistorialPedido> HistorialPedidos { get; set; } = new List<HistorialPedido>();

    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
