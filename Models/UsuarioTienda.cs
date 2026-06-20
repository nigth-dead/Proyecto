namespace Proyecto.Models;

public partial class UsuarioTienda
{
    public int UsuarioTiendaId { get; set; }

    public int UsuarioId { get; set; }

    public int TiendaId { get; set; }

    public virtual Usuario Usuario { get; set; } = null!;

    public virtual Tienda Tienda { get; set; } = null!;
}