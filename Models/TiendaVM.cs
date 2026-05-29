namespace Proyecto.Models
{
    public class TiendaVM
    {
        public int TiendaId { get; set; }

        public string Nombre { get; set; } = null!;

        public string Direccion { get; set; } = null!;

        public bool? Estado { get; set; }

        public decimal VentaTotal { get; set; }
    }
}