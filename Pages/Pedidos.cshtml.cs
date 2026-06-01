using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;
using System.Text.Json;

namespace Proyecto.Pages;



public class PedidosModel : PageModel
{
    public Tienda? TiendaActual { get; set; }
    public Usuario? UsuarioActual { get; set; }
    private punto_de_ventaContext? dbContext;
    public List<HistorialPedido> Pedidos { get; set; } = new();
    [BindProperty]
    public String Estado { get; set; } = "";
    [BindProperty]
    public int PedidoId { get; set; }

    public async Task OnGetAsync()
    {
        CargarTiendaActual();
        CargarUsuarioActual();
        using(dbContext = new punto_de_ventaContext())
        {
            if (TiendaActual != null)
            {
                Pedidos = await dbContext.HistorialPedido
                .Include(p => p.Proveedor)
                .Include(p => p.Usuario)
                .Include(p => p.HistorialPedidoDetalle)
                .ThenInclude(p => p.Producto)
                .Where(i => i.TiendaId == TiendaActual.TiendaId)
                .ToListAsync();
            }
        }
    }

    private void CargarTiendaActual()
    {
        var json = HttpContext.Session.GetString("Tienda");

        Console.WriteLine("Contenido de la sesión Tienda: " + json);

        if (json != null)
        {
            TiendaActual = JsonSerializer.Deserialize<Tienda>(json);
        }
    }

    private void CargarUsuarioActual()
    {
        var Json = HttpContext.Session.GetString("Usuario");
        if (Json != null)
        {
            UsuarioActual =
            JsonSerializer.Deserialize<Usuario>(Json);
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        using(dbContext = new punto_de_ventaContext())
        {
            HistorialPedido? pedido = await dbContext.HistorialPedido.FirstOrDefaultAsync(p => p.PedidoId == PedidoId);
            pedido?.Estado =Estado;
            dbContext.SaveChanges();
        }
        return RedirectToPage();
    }
}
