using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class HistorialPedidoDetalle
{
    public int PedidoId { get; set; }

    public int ProductoId { get; set; }

    public int Cantidad { get; set; }

    public decimal CostoUnitario { get; set; }

    public virtual HistorialPedido Pedido { get; set; } = null!;

    public virtual Producto Producto { get; set; } = null!;
}
