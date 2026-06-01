using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Proyecto.Models;
using System.Text.Json;

namespace Proyecto.Pages;

public class DetallesTiendaModel : PageModel
{
    public Usuario? UsuarioActual { get; set; }
    private punto_de_ventaContext? dbContext;
    public List<Producto> Productos { get; set; } = new();
    public List<Proveedor> Proveedores { get; set; } = new();
    public List<Inventario> Inventarios { get; set; } = new();
    public Tienda? TiendaActual { get; set; }
    [BindProperty]
    public int InventarioId { get; set; }
    [BindProperty]
    public int Cantidad { get; set; }
    [BindProperty]
    public String? Motivo { get; set;} = "";
    /*Datos de nuevo pedido*/
    [BindProperty]
    public int ProveedorId { get; set; }
    [BindProperty]
    public List<int> ProductoIds { get; set; } = new();
    [BindProperty]
    public List<int> Cantidades { get; set; } = new();
    [BindProperty]
    public List<decimal> CostosUnitarios { get; set; } = new();


    public async Task OnGetAsync()
    {
        await CargarDatos();
    }

    public async Task<IActionResult> OnPostRegistrarPedidoAsync()
    {
        if (ProductoIds.Count != ProductoIds.Distinct().Count())
        {
            await CargarDatos();
            return Page();
        }
        if(Cantidades.Any(c => c <= 0))
        {
            return Page();
        }
        if (CostosUnitarios.Any(c => c <= 0))
        {
            return Page();
        }
        await CargarDatos();
        if(TiendaActual == null || UsuarioActual == null)
        {
            return Page();
        }
        using (dbContext = new punto_de_ventaContext())
        {
            decimal MontoTotal = 0;
            foreach(decimal costo in CostosUnitarios)
            {
                MontoTotal += costo;
            }
            HistorialPedido Pedido = new HistorialPedido()
            {
                TiendaId = TiendaActual.TiendaId,
                ProveedorId = ProveedorId,
                UsuarioId = UsuarioActual.UsuarioId,
                Fecha = DateTime.Now,
                MontoTotal = MontoTotal,
                Estado = "Pendiente"
            };
            dbContext.Add(Pedido);
            await dbContext.SaveChangesAsync();
            for(int i = 0; i < ProductoIds.Count; i++)
            {
                HistorialPedidoDetalle detalle = new HistorialPedidoDetalle()
                {
                    PedidoId = Pedido.PedidoId,
                    ProductoId = ProductoIds[i],
                    Cantidad = Cantidades[i],
                    CostoUnitario = CostosUnitarios[i]
                };
                dbContext.Add(detalle);
            }
            await dbContext.SaveChangesAsync();
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostReducirStockAsync()
    {
        await CargarDatos();
        if(UsuarioActual == null)
        {
            return RedirectToPage();
        }
        using (dbContext = new punto_de_ventaContext())
        {
            Inventario? Inventario = await dbContext.Inventario.FirstOrDefaultAsync(i => i.InventarioId == InventarioId);
            if (Inventario == null)
            {
                await CargarDatos();
                return Page();
            }
            if(Cantidad <= 0)
            {
                await CargarDatos();
                return Page();
            }
            if (string.IsNullOrWhiteSpace(Motivo))
            {
                await CargarDatos();
                return Page();
            }
            if (Inventario.Stock < Cantidad)
            {
                ModelState.AddModelError("", "No hay suficiente Stock");
                await CargarDatos();
                return Page();
            }
            Inventario.Stock -= Cantidad;
            HistorialMovimiento NuevoMovimiento = new HistorialMovimiento()
            {
                InventarioId = InventarioId,
                UsuarioId = UsuarioActual.UsuarioId,
                Tipo = "Ajuste",
                Cantidad = Cantidad,
                Motivo = Motivo,
                Fecha = DateTime.Now
            };
            dbContext.Add(NuevoMovimiento);
            await dbContext.SaveChangesAsync();
        }
        return RedirectToPage();
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

    private async Task CargarProveedores()
    {
        if(dbContext != null)
        {
            Proveedores = await dbContext.Proveedor.ToListAsync();
        }
    }
    
    private async Task CargarDatos()
    {
        CargarTiendaActual();
        CargarUsuarioActual();
        using (dbContext = new punto_de_ventaContext())
        {
            await CargarProveedores();
            if (TiendaActual != null)
            {
                Inventarios = await dbContext.Inventario
                .Include(i => i.Producto)
                .ThenInclude(p => p.Categoria)
                .Where(i => i.TiendaId == TiendaActual.TiendaId)
                .ToListAsync();
            }
        }
    }
}
