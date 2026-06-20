namespace Proyecto.Models;

public partial class Pago
{
    public int PagoId { get; set; }

    public int? VentaId { get; set; }

    public decimal Monto { get; set; }

    public string Metodo { get; set; } = null!;

    public virtual Venta? Venta { get; set; }
}