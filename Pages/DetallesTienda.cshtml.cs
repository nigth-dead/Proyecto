using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;
using System.Text.Json;

namespace Proyecto.Pages;

public class DetallesTiendaModel : PageModel
{
    public Usuario? UsuarioActual { get; set; }
    private punto_de_ventaContext? dbContext;
    public List<Producto> Productos { get; set; } = new();
    public List<Inventario>? Inventarios { get; set; } = new();
    public Tienda? TiendaActual { get; set; }
    public async Task OnGetAsync()
    {
        CargarTiendaActual();
        using (dbContext = new punto_de_ventaContext())
        {
            if (TiendaActual != null)
            {
                Inventarios = dbContext.Inventario
                .Include(i => i.Producto)
                .ThenInclude(p => p.Categoria)
                .Where(i => i.TiendaId == TiendaActual.TiendaId)
                .ToList();
            }
        }
        var Json = HttpContext.Session.GetString("Usuario");
        if (Json != null)
        {
            UsuarioActual =
            JsonSerializer.Deserialize<Usuario>(Json);
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
}
