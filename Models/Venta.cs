using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Venta
{
    public int VentaId { get; set; }

    public int TiendaId { get; set; }

    public int CorteId { get; set; }

    public int UsuarioId { get; set; }

    public DateTime Fecha { get; set; }

    public decimal Total { get; set; }

    public virtual CorteCaja Corte { get; set; } = null!;

    public virtual ICollection<DetalleVenta> DetalleVenta { get; set; } = new List<DetalleVenta>();

    public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();

    public virtual Usuario Usuario { get; set; } = null!;
}
