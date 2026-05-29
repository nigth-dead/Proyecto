using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class HistorialMovimiento
{
    public int MovimientoId { get; set; }

    public int InventarioId { get; set; }

    public int? UsuarioId { get; set; }

    public string Tipo { get; set; } = null!;

    public int Cantidad { get; set; }

    public string? Motivo { get; set; }

    public DateTime Fecha { get; set; }

    public virtual Inventario Inventario { get; set; } = null!;

    public virtual Usuario? Usuario { get; set; }
}
