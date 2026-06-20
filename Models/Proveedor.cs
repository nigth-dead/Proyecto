using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Proveedor
{
    public int ProveedorId { get; set; }

    public string Nombre { get; set; } = null!;

    public string Telefono { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public bool? Activo { get; set; }

    public virtual ICollection<HistorialPedido> HistorialPedido { get; set; } = new List<HistorialPedido>();

    public virtual ICollection<Producto> Producto { get; set; } = new List<Producto>();
}
