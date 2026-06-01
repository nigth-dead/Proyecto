using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;
using System.Text.Json;

namespace Proyecto.Pages;

public class TiendasModel : PageModel
{
    private readonly punto_de_ventaContext _context;
    public Usuario? UsuarioActual { get; set; }
    public List<TiendaVM> Tiendas { get; set; } = new();
    public List<Venta> Ventas { get; set; } = new();
    public TiendasModel(punto_de_ventaContext context)
    {
        _context = context;
    }
    public async Task OnGetAsync()
    {
        var Json = HttpContext.Session.GetString("Usuario");
        if (Json != null)
        {
            UsuarioActual = JsonSerializer.Deserialize<Usuario>(Json);
        }

        var tiendas = await _context.Tienda.ToListAsync();

        var totales = await _context.Venta
            .GroupBy(v => v.TiendaId)
            .Select(t => new
            {
                TiendaId = t.Key,
                Total = t.Sum(v => v.Total)
            })
            .ToListAsync();

        Tiendas = tiendas.Select(t =>
        {
            var total = totales.FirstOrDefault(x => x.TiendaId == t.TiendaId);

            return new TiendaVM
            {
                TiendaId = t.TiendaId,
                Nombre = t.Nombre,
                Direccion = t.Direccion,
                Estado = t.Estado,
                VentaTotal = total?.Total ?? 0
            };
        }).ToList();
    }

    public IActionResult OnGetAbrirEmpleados(int id)
    {
        var tienda = _context.Tienda.FirstOrDefault(t => t.TiendaId == id);
        if (tienda != null)
        {
            HttpContext.Session.SetString("Tienda", JsonSerializer.Serialize(tienda));
        }
        return RedirectToPage("/Empleados");
    }

    public IActionResult OnGetAbrirInventario(int id)
    {
        var tienda = _context.Tienda.FirstOrDefault(t => t.TiendaId == id);
        if (tienda != null)
        {
            HttpContext.Session.SetString("Tienda", JsonSerializer.Serialize(tienda));
        }
        return RedirectToPage("/DetallesTienda");
    }
}
