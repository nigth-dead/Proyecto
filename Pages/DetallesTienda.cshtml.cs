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
    public String? Motivo { get; set; } = "";
    /*Datos de nuevo pedido*/
    [BindProperty]
    public int ProveedorId { get; set; }
    [BindProperty]
    public List<int> ProductoIds { get; set; } = new();
    [BindProperty]
    public List<int> Cantidades { get; set; } = new();
    [BindProperty]
    public List<decimal> CostosUnitarios { get; set; } = new();

    [TempData]
    public string? Mensaje { get; set; }
    [TempData]
    public string? TipoMensaje { get; set; }
    [TempData]
    public string? TituloMensaje { get; set; }


    public async Task OnGetAsync()
    {
        await CargarDatos();
    }

    public async Task<IActionResult> OnPostRegistrarPedidoAsync()
    {
        if (ProductoIds.Count == 0 ||
            ProductoIds.Count != Cantidades.Count ||
            ProductoIds.Count != CostosUnitarios.Count)
        {
            TituloMensaje = "Pedido incompleto";
            Mensaje = "Debes agregar al menos un producto y completar todos los campos.";
            TipoMensaje = "warning";
            return RedirectToPage();
        }
        if (ProductoIds.Count != ProductoIds.Distinct().Count())
        {
            TituloMensaje = "Producto repetido";
            Mensaje = "No puedes registrar el mismo producto más de una vez en el pedido.";
            TipoMensaje = "warning";
            return RedirectToPage();
        }
        if (Cantidades.Any(c => c <= 0))
        {
            TituloMensaje = "Cantidad inválida";
            Mensaje = "La cantidad de cada producto debe ser mayor a 0.";
            TipoMensaje = "warning";
            return RedirectToPage();
        }
        if (CostosUnitarios.Any(c => c <= 0))
        {
            TituloMensaje = "Costo inválido";
            Mensaje = "El costo unitario de cada producto debe ser mayor a 0.";
            TipoMensaje = "warning";
            return RedirectToPage();
        }
        await CargarDatos();
        if (TiendaActual == null)
        {
            TituloMensaje = "Tienda no encontrada";
            Mensaje = "No se pudo identificar la tienda actual.";
            TipoMensaje = "danger";
            return RedirectToPage();
        }
        if (UsuarioActual == null)
        {
            TituloMensaje = "Usuario no encontrado";
            Mensaje = "No se pudo identificar el usuario actual.";
            TipoMensaje = "danger";
            return RedirectToPage();
        }
        using (dbContext = new punto_de_ventaContext())
        {
            decimal MontoTotal = 0;
            for (int i = 0; i < ProductoIds.Count; i++)
            {
                MontoTotal += Cantidades[i] * CostosUnitarios[i];
            }
            HistorialPedido pedido = new HistorialPedido()
            {
                TiendaId = TiendaActual.TiendaId,
                ProveedorId = ProveedorId,
                UsuarioId = UsuarioActual.UsuarioId,
                Fecha = DateTime.Now,
                MontoTotal = MontoTotal,
                Estado = "Pendiente"
            };
            dbContext.Add(pedido);
            await dbContext.SaveChangesAsync();
            for (int i = 0; i < ProductoIds.Count; i++)
            {
                HistorialPedidoDetalle detalle = new HistorialPedidoDetalle()
                {
                    PedidoId = pedido.PedidoId,
                    ProductoId = ProductoIds[i],
                    Cantidad = Cantidades[i],
                    CostoUnitario = CostosUnitarios[i]
                };
                dbContext.Add(detalle);
            }
            await dbContext.SaveChangesAsync();
        }
        TituloMensaje = "Pedido registrado";
        Mensaje = "El pedido se registró correctamente.";
        TipoMensaje = "success";
        return RedirectToPage();
}

    public async Task<IActionResult> OnPostReducirStockAsync()
    {
        await CargarDatos();
        if (UsuarioActual == null)
        {
            TituloMensaje = "Usuario no encontrado";
            Mensaje = "No se pudo identificar el usuario actual.";
            TipoMensaje = "danger";

            return RedirectToPage();
        }

        using (dbContext = new punto_de_ventaContext())
        {
            Inventario? inventario = await dbContext.Inventario
                .FirstOrDefaultAsync(i => i.InventarioId == InventarioId);

            if (inventario == null)
            {
                TituloMensaje = "Reduccion no registrada";
                Mensaje = "No se encontró el producto en el inventario.";
                TipoMensaje = "danger";

                return RedirectToPage();
            }

            if (Cantidad <= 0)
            {
                TituloMensaje = "Reduccion no registrada";
                Mensaje = "La cantidad a reducir debe ser mayor a 0.";
                TipoMensaje = "warning";

                return RedirectToPage();
            }

            if (string.IsNullOrWhiteSpace(Motivo))
            {
                TituloMensaje = "Reduccion no registrada";
                Mensaje = "Debes escribir una razón para la reducción de stock.";
                TipoMensaje = "warning";

                return RedirectToPage();
            }

            if (inventario.Stock < Cantidad)
            {
                TituloMensaje = "Reduccion no registrada";
                Mensaje = $"No hay suficiente stock. Stock actual: {inventario.Stock}.";
                TipoMensaje = "warning";

                return RedirectToPage();
            }

            inventario.Stock -= Cantidad;

            HistorialMovimiento nuevoMovimiento = new HistorialMovimiento()
            {
                InventarioId = InventarioId,
                UsuarioId = UsuarioActual.UsuarioId,
                Tipo = "Ajuste",
                Cantidad = Cantidad,
                Motivo = Motivo,
                Fecha = DateTime.Now
            };

            dbContext.Add(nuevoMovimiento);
            await dbContext.SaveChangesAsync();

            TituloMensaje = "Stock actualizado";
            Mensaje = "La reducción de stock se registró correctamente.";
            TipoMensaje = "success";
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
        if (dbContext != null)
        {
            Proveedores = await dbContext.Proveedor.ToListAsync();
        }
    }

    public async Task CargarProductos()
    {
        if (dbContext != null)
        {
            Productos = await dbContext.Producto.ToListAsync();
        }
    }

    private async Task CargarDatos()
    {
        CargarTiendaActual();
        CargarUsuarioActual();
        using (dbContext = new punto_de_ventaContext())
        {
            await CargarProveedores();
            await CargarProductos();
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
