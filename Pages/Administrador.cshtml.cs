using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using Proyecto.Models;
using Microsoft.EntityFrameworkCore;

namespace Proyecto.Pages;

public class AdministradorModel : PageModel
{
    public Usuario? UsuarioActual { get; set; }

    private punto_de_ventaContext? dbContext;

    /*Tarjeta de ventas*/
    private List<Venta> Ventas { get; set; } = new();
    private List<Venta> VentasDeAyer { get; set; } = new();

    public decimal TotalVentasDelDia { get; set; }
    public decimal TotalVentasDeAyer { get; set; }
    public decimal Porcentaje { get; set; }

    /*Tarjeta de productos en stock*/
    public int TotalProductos { get; set; }
    public int ProductosBajosDeStock { get; set; }

    /*Tienda con mas ventas del mes*/
    public Tienda? TiendaConMasVentas { get; set; }
    public decimal MasVentasDelMes { get; set; }

    /*Venta promedio del mes*/
    public decimal Promedio { get; set; }

    /*Porcentaje de ventas por categoria*/
    public List<(string Categoria, decimal PorcentajeCategoria)> PorcentajesCategorias { get; set; } = new();

    public List<Inventario> ProductosSinStock { get; set; } = new();

    public List<Venta> UltimasVentasDelDia { get; set; } = new();

    public async Task<IActionResult> OnGet()
    {
        var validacion = ValidarAdministrador();

        if (validacion != null)
        {
            return validacion;
        }

        await CalcularVentasDelDia();
        await ContarStock();
        await BuscarTiendaConMasVentas();
        await CalcularPromedioDeVentaDelMes();
        await CalcularVentasPorCategoria();
        await AlertasDeStock();
        await BuscarUltimasVentasDelDia();

        return Page();
    }

    private IActionResult? ValidarAdministrador()
    {
        var json = HttpContext.Session.GetString("Usuario");

        if (json == null)
        {
            return RedirectToPage("/Index");
        }

        UsuarioActual = JsonSerializer.Deserialize<Usuario>(json);

        if (UsuarioActual == null)
        {
            HttpContext.Session.Remove("Usuario");
            HttpContext.Session.Remove("Tienda");

            return RedirectToPage("/Index");
        }

        if (UsuarioActual.Contratado != true)
        {
            HttpContext.Session.Remove("Usuario");
            HttpContext.Session.Remove("Tienda");

            return RedirectToPage("/Index");
        }

        if (UsuarioActual.Rol != "Administrador")
        {
            return RedirectToPage("/Index");
        }

        return null;
    }

    private async Task CalcularVentasDelDia()
    {
        using (dbContext = new punto_de_ventaContext())
        {
            Ventas = await dbContext.Venta
                .Where(v => v.Fecha > DateTime.Today)
                .ToListAsync();

            foreach (var v in Ventas)
            {
                TotalVentasDelDia += v.Total;
            }

            VentasDeAyer = await dbContext.Venta
                .Where(v => v.Fecha > DateTime.Today.AddDays(-1) && v.Fecha < DateTime.Today)
                .ToListAsync();

            foreach (var v in VentasDeAyer)
            {
                TotalVentasDeAyer += v.Total;
            }

            if (TotalVentasDeAyer > 0)
            {
                Porcentaje = ((TotalVentasDelDia - TotalVentasDeAyer) * 100) / TotalVentasDeAyer;
            }
        }
    }

    public async Task ContarStock()
    {
        using (dbContext = new punto_de_ventaContext())
        {
            TotalProductos = await dbContext.Producto
                .CountAsync();

            ProductosBajosDeStock = await dbContext.Producto
                .Include(p => p.Inventario)
                .Where(p => p.Inventario.Any(i => i.Stock < 10))
                .CountAsync();
        }
    }

    public async Task BuscarTiendaConMasVentas()
    {
        using (dbContext = new punto_de_ventaContext())
        {
            var MasVentas = await dbContext.Venta
                .Where(v => v.Fecha > DateTime.Today.AddMonths(-1))
                .GroupBy(v => v.TiendaId)
                .Select(g => new
                {
                    TiendaId = g.Key,
                    TotalVentas = g.Count()
                })
                .OrderByDescending(x => x.TotalVentas)
                .FirstOrDefaultAsync();

            if (MasVentas != null)
            {
                MasVentasDelMes = MasVentas.TotalVentas;

                TiendaConMasVentas = await dbContext.Tienda
                    .Where(t => t.TiendaId == MasVentas.TiendaId)
                    .FirstOrDefaultAsync();
            }
        }
    }

    public async Task CalcularPromedioDeVentaDelMes()
    {
        using (dbContext = new punto_de_ventaContext())
        {
            Promedio = await dbContext.Venta
                .Where(v => v.Fecha > DateTime.Today.AddMonths(-1))
                .Select(v => (decimal?)v.Total)
                .AverageAsync() ?? 0;
        }
    }

    public async Task CalcularVentasPorCategoria()
    {
        using (dbContext = new punto_de_ventaContext())
        {
            var ventasCategoria = await dbContext.DetalleVenta
                .Where(v => v.Venta.Fecha > DateTime.Today.AddMonths(-1))
                .Include(d => d.Producto)
                    .ThenInclude(p => p.Categoria)
                .Where(d => d.Producto.Categoria != null)
                .GroupBy(d => d.Producto.Categoria!.Nombre)
                .Select(g => new
                {
                    Categoria = g.Key,
                    Cantidad = g.Sum(d => d.Cantidad)
                })
                .ToListAsync();

            int totalGeneral = ventasCategoria.Sum(c => c.Cantidad);

            PorcentajesCategorias = ventasCategoria.Select(c => (
                Categoria: c.Categoria,
                PorcentajeCategoria: totalGeneral > 0
                    ? (c.Cantidad * 100m) / totalGeneral
                    : 0
            )).ToList();
        }
    }

    public async Task AlertasDeStock()
    {
        using (dbContext = new punto_de_ventaContext())
        {
            ProductosSinStock = await dbContext.Inventario
                .AsNoTracking()
                .Where(i => i.Stock < 10)
                .Include(i => i.Producto)
                .Include(i => i.Tienda)
                .ToListAsync();
        }
    }

    public async Task BuscarUltimasVentasDelDia()
    {
        using (dbContext = new punto_de_ventaContext())
        {
            UltimasVentasDelDia = await dbContext.Venta
                .Where(v => v.Fecha > DateTime.Today)
                .OrderByDescending(v => v.Fecha)
                .Take(10)
                .ToListAsync();
        }
    }
}