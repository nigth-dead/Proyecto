using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class HistorialPedido
{
    public int PedidoId { get; set; }

    public int TiendaId { get; set; }

    public int ProveedorId { get; set; }

    public int? UsuarioId { get; set; }

    public DateTime Fecha { get; set; }

    public decimal MontoTotal { get; set; }

    public string Estado { get; set; } = null!;

    public virtual ICollection<HistorialPedidoDetalle> HistorialPedidoDetalle { get; set; } = new List<HistorialPedidoDetalle>();

    public virtual Proveedor Proveedor { get; set; } = null!;

    public virtual Tienda Tienda { get; set; } = null!;

    public virtual Usuario Usuario { get; set; } = new();
}
