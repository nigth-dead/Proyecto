using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Pago
{
    public int PagoId { get; set; }

    public int VentaId { get; set; }

    public decimal Monto { get; set; }

    public string Metodo { get; set; } = null!;

    public bool Procesado { get; set; }

    public DateTime Fecha { get; set; }

    public virtual Venta Venta { get; set; } = null!;
}
