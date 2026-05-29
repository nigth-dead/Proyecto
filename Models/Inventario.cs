using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Inventario
{
    public int InventarioId { get; set; }

    public int TiendaId { get; set; }

    public int ProductoId { get; set; }

    public int Stock { get; set; }

    public virtual ICollection<HistorialMovimiento> HistorialMovimientos { get; set; } = new List<HistorialMovimiento>();

    public virtual Producto Producto { get; set; } = null!;

    public virtual Tienda Tienda { get; set; } = null!;
}
