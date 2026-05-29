using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class CorteCaja
{
    public int CorteId { get; set; }

    public int TiendaId { get; set; }

    public int UsuarioId { get; set; }

    public DateTime FechaApertura { get; set; }

    public DateTime? FechaCierre { get; set; }

    public decimal SaldoInicial { get; set; }

    public decimal? SaldoEsperado { get; set; }

    public decimal? SaldoFinal { get; set; }

    public string Estado { get; set; } = null!;

    public virtual Tienda Tienda { get; set; } = null!;

    public virtual Usuario Usuario { get; set; } = null!;

    public virtual ICollection<Venta> Venta { get; set; } = new List<Venta>();
}
