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
    public string Estado { get; set; } = "";

    [BindProperty]
    public int PedidoId { get; set; }

    public List<HistorialPedidoDetalle> Detalles { get; set; } = new();

    [TempData]
    public string? Mensaje { get; set; }

    [TempData]
    public string? TipoMensaje { get; set; }

    [TempData]
    public string? TituloMensaje { get; set; }

    [BindProperty]
    public string ParametroDeBusqueda { get; set; } = "";

    public async Task<IActionResult> OnGetAsync()
    {
        var validacion = ValidarAdministrador();

        if (validacion != null)
        {
            return validacion;
        }

        await CargarTiendaActualAsync();

        if (TiendaActual == null)
        {
            return RedirectToPage("/Tiendas");
        }

        await CargarPedidosAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var validacion = ValidarAdministrador();

        if (validacion != null)
        {
            return validacion;
        }

        await CargarTiendaActualAsync();

        if (TiendaActual == null)
        {
            TituloMensaje = "Tienda no encontrada";
            Mensaje = "No se pudo identificar la tienda actual.";
            TipoMensaje = "danger";

            return RedirectToPage();
        }

        if (TiendaActual.Estado != true)
        {
            TituloMensaje = "Tienda inactiva";
            Mensaje = "No se pueden actualizar pedidos de una tienda inactiva.";
            TipoMensaje = "warning";

            return RedirectToPage();
        }

        Estado = Estado?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(Estado))
        {
            TituloMensaje = "Sin cambios";
            Mensaje = "No se seleccionó ningún estado para actualizar.";
            TipoMensaje = "warning";

            return RedirectToPage();
        }

        if (Estado != "Pendiente" && Estado != "Completado" && Estado != "Cancelado")
        {
            TituloMensaje = "Estado inválido";
            Mensaje = "El estado seleccionado no es válido.";
            TipoMensaje = "danger";

            return RedirectToPage();
        }

        using (dbContext = new punto_de_ventaContext())
        {
            using var transaccion = await dbContext.Database.BeginTransactionAsync();

            try
            {
                HistorialPedido? pedido = await dbContext.HistorialPedido
                    .FirstOrDefaultAsync(p =>
                        p.PedidoId == PedidoId &&
                        p.TiendaId == TiendaActual.TiendaId);

                if (pedido == null)
                {
                    TituloMensaje = "Pedido no encontrado";
                    Mensaje = "No se encontró el pedido que intentas actualizar.";
                    TipoMensaje = "danger";

                    await transaccion.RollbackAsync();
                    return RedirectToPage();
                }

                if (pedido.Estado != "Pendiente")
                {
                    TituloMensaje = "Pedido cerrado";
                    Mensaje = "Este pedido ya fue completado o cancelado, por lo que no se puede modificar.";
                    TipoMensaje = "warning";

                    await transaccion.RollbackAsync();
                    return RedirectToPage();
                }

                if (pedido.Estado == Estado)
                {
                    TituloMensaje = "Sin cambios";
                    Mensaje = "El pedido ya tenía ese estado.";
                    TipoMensaje = "info";

                    await transaccion.RollbackAsync();
                    return RedirectToPage();
                }

                pedido.Estado = Estado;

                if (pedido.Estado == "Completado")
                {
                    Detalles = await dbContext.HistorialPedidoDetalle
                        .Include(d => d.Producto)
                        .Where(d => d.PedidoId == pedido.PedidoId)
                        .ToListAsync();

                    if (!Detalles.Any())
                    {
                        TituloMensaje = "Pedido sin detalles";
                        Mensaje = "No se puede completar el pedido porque no tiene productos registrados.";
                        TipoMensaje = "danger";

                        await transaccion.RollbackAsync();
                        return RedirectToPage();
                    }

                    foreach (var detalle in Detalles)
                    {
                        if (detalle.Cantidad <= 0)
                        {
                            TituloMensaje = "Cantidad inválida";
                            Mensaje = "Uno de los productos del pedido tiene una cantidad inválida.";
                            TipoMensaje = "danger";

                            await transaccion.RollbackAsync();
                            return RedirectToPage();
                        }

                        var inventario = await dbContext.Inventario
                            .FirstOrDefaultAsync(i =>
                                i.TiendaId == pedido.TiendaId &&
                                i.ProductoId == detalle.ProductoId);

                        if (inventario != null)
                        {
                            inventario.Stock += detalle.Cantidad;
                        }
                        else
                        {
                            inventario = new Inventario()
                            {
                                TiendaId = pedido.TiendaId,
                                ProductoId = detalle.ProductoId,
                                Stock = detalle.Cantidad
                            };

                            dbContext.Inventario.Add(inventario);

                            await dbContext.SaveChangesAsync();
                        }

                        HistorialMovimiento movimiento = new HistorialMovimiento()
                        {
                            InventarioId = inventario.InventarioId,
                            UsuarioId = UsuarioActual!.UsuarioId,
                            Tipo = "Entrada",
                            Cantidad = detalle.Cantidad,
                            Motivo = $"Pedido {pedido.PedidoId} completado",
                            Fecha = DateTime.Now
                        };

                        dbContext.HistorialMovimiento.Add(movimiento);
                    }

                    TituloMensaje = "Pedido completado";
                    Mensaje = "El pedido se marcó como completado y el stock fue actualizado correctamente.";
                    TipoMensaje = "success";
                }
                else if (pedido.Estado == "Cancelado")
                {
                    TituloMensaje = "Pedido cancelado";
                    Mensaje = "El pedido se canceló correctamente.";
                    TipoMensaje = "warning";
                }
                else
                {
                    TituloMensaje = "Estado actualizado";
                    Mensaje = "El estado del pedido se actualizó correctamente.";
                    TipoMensaje = "success";
                }

                await dbContext.SaveChangesAsync();
                await transaccion.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaccion.RollbackAsync();

                string errorReal = ex.InnerException != null ? ex.InnerException.Message : ex.Message;

                TituloMensaje = "Error al actualizar";
                Mensaje = "No se pudo actualizar el pedido. Error: " + errorReal;
                TipoMensaje = "danger";
            }
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostBusquedaAsync()
    {
        var validacion = ValidarAdministrador();

        if (validacion != null)
        {
            return validacion;
        }

        await CargarTiendaActualAsync();

        if (TiendaActual == null)
        {
            return RedirectToPage("/Tiendas");
        }

        await CargarPedidosAsync(ParametroDeBusqueda);

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

    private async Task CargarTiendaActualAsync()
    {
        TiendaActual = null;

        var json = HttpContext.Session.GetString("Tienda");

        if (json == null)
        {
            return;
        }

        var tiendaSesion = JsonSerializer.Deserialize<Tienda>(json);

        if (tiendaSesion == null)
        {
            return;
        }

        using (dbContext = new punto_de_ventaContext())
        {
            TiendaActual = await dbContext.Tienda
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TiendaId == tiendaSesion.TiendaId);
        }
    }

    private async Task CargarPedidosAsync(string busqueda = "")
    {
        using (dbContext = new punto_de_ventaContext())
        {
            if (TiendaActual == null)
            {
                Pedidos = new List<HistorialPedido>();
                return;
            }

            busqueda = busqueda?.Trim() ?? "";

            IQueryable<HistorialPedido> consulta = dbContext.HistorialPedido
                .AsNoTracking()
                .Include(p => p.Proveedor)
                .Include(p => p.Usuario)
                .Include(p => p.HistorialPedidoDetalle)
                    .ThenInclude(d => d.Producto)
                .Where(p => p.TiendaId == TiendaActual.TiendaId);

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                consulta = consulta.Where(p =>
                    p.PedidoId.ToString().Contains(busqueda) ||
                    p.Estado.Contains(busqueda) ||
                    (
                        p.Usuario != null &&
                        p.Usuario.Nombre.Contains(busqueda)
                    ) ||
                    (
                        p.Proveedor != null &&
                        p.Proveedor.Nombre.Contains(busqueda)
                    ) ||
                    p.HistorialPedidoDetalle.Any(d =>
                        d.Producto != null &&
                        (
                            d.Producto.Nombre.Contains(busqueda) ||
                            d.Producto.Codigo.Contains(busqueda)
                        )
                    ));
            }

            Pedidos = await consulta
                .OrderByDescending(p => p.Fecha)
                .ToListAsync();
        }
    }
}