using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Producto
{
    public int ProductoId { get; set; }

    public int? ProveedorId { get; set; }

    public string Nombre { get; set; } = null!;

    public string Codigo { get; set; } = null!;

    public decimal Precio { get; set; }

    public bool? Activo { get; set; }

    public int? CategoriaId { get; set; }

    public virtual Categoria? Categoria { get; set; }

    public virtual ICollection<DetalleVenta> DetalleVenta { get; set; } = new List<DetalleVenta>();

    public virtual ICollection<HistorialPedidoDetalle> HistorialPedidoDetalle { get; set; } = new List<HistorialPedidoDetalle>();

    public virtual ICollection<Inventario> Inventario { get; set; } = new List<Inventario>();

    public virtual Proveedor? Proveedor { get; set; }
}
