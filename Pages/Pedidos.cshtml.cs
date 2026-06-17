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
    public List<HistorialPedidoDetalle> Detalles { get; set; } = new();

    [TempData]
    public string? Mensaje { get; set; }
    [TempData]
    public string? TipoMensaje { get; set; }
    [TempData]
    public string? TituloMensaje { get; set; }
    [BindProperty]
    public string ParametroDeBusqueda { get; set; } = "";

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
                .ThenInclude(p => p.Inventario)
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
        if (!string.IsNullOrWhiteSpace(Estado))
        {
            Estado = Estado.Trim();

            if (Estado != "Pendiente" && Estado != "Completado" && Estado != "Cancelado")
            {
                TituloMensaje = "Estado inválido";
                Mensaje = "El estado seleccionado no es válido.";
                TipoMensaje = "danger";

                return RedirectToPage();
            }

            using (dbContext = new punto_de_ventaContext())
            {
                HistorialPedido? pedido = await dbContext.HistorialPedido
                    .FirstOrDefaultAsync(p => p.PedidoId == PedidoId);

                if (pedido == null)
                {
                    TituloMensaje = "Pedido no encontrado";
                    Mensaje = "No se encontró el pedido que intentas actualizar.";
                    TipoMensaje = "danger";

                    return RedirectToPage();
                }

                if (pedido.Estado != Estado)
                {
                    pedido.Estado = Estado;

                    if (pedido.Estado == "Completado")
                    {
                        Detalles = await dbContext.HistorialPedidoDetalle
                            .Include(d => d.Producto)
                            .ThenInclude(d => d.Inventario)
                            .Where(d => d.PedidoId == pedido.PedidoId)
                            .ToListAsync();

                        foreach (var detalle in Detalles)
                        {
                            var Inventario = detalle.Producto.Inventario
                                .FirstOrDefault(i => i.TiendaId == pedido.TiendaId);

                            if (Inventario != null)
                            {
                                Inventario.Stock += detalle.Cantidad;
                            }
                            else
                            {
                                Inventario NuevoInventario = new Inventario()
                                {
                                    TiendaId = pedido.TiendaId,
                                    ProductoId = detalle.ProductoId,
                                    Stock = detalle.Cantidad
                                };

                                dbContext.Add(NuevoInventario);
                            }
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
                }
                else
                {
                    TituloMensaje = "Sin cambios";
                    Mensaje = "El pedido ya tenía ese estado.";
                    TipoMensaje = "info";
                }
            }
        }
        else
        {
            TituloMensaje = "Sin cambios";
            Mensaje = "No se seleccionó ningún estado para actualizar.";
            TipoMensaje = "warning";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostBusquedaAsync()
    {
        CargarTiendaActual();
        using(dbContext = new punto_de_ventaContext())
        {
            if (TiendaActual != null)
            {
                if (string.IsNullOrWhiteSpace(ParametroDeBusqueda))
                {
                    Pedidos = await dbContext.HistorialPedido
                    .Include(p => p.Proveedor)
                    .Include(p => p.Usuario)
                    .Include(p => p.HistorialPedidoDetalle)
                    .ThenInclude(p => p.Producto)
                    .ThenInclude(p => p.Inventario)
                    .Where(i => i.TiendaId == TiendaActual.TiendaId)
                    .ToListAsync();
                }
                else
                {
                    Pedidos = await dbContext.HistorialPedido
                    .Where(i => i.TiendaId == TiendaActual.TiendaId)
                    .Where(p => p.PedidoId.ToString().Contains(ParametroDeBusqueda)||
                    p.Usuario.Nombre.Contains(ParametroDeBusqueda)||
                    p.Proveedor.Nombre.Contains(ParametroDeBusqueda))
                    .Include(p => p.Proveedor)
                    .Include(p => p.Usuario)
                    .Include(p => p.HistorialPedidoDetalle)
                    .ThenInclude(p => p.Producto)
                    .ThenInclude(p => p.Inventario)
                    .ToListAsync();
                }
            }
        }
        return Page();
    }
}
