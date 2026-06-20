using Microsoft.AspNetCore.Mvc;
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
    public List<Proveedor> Proveedores { get; set; } = new();
    public List<Inventario> Inventarios { get; set; } = new();
    public Tienda? TiendaActual { get; set; }

    [BindProperty]
    public int InventarioId { get; set; }

    [BindProperty]
    public int Cantidad { get; set; }

    [BindProperty]
    public string? Motivo { get; set; } = "";

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

    [BindProperty]
    public string ParametroDeBusqueda { get; set; } = "";

    public async Task<IActionResult> OnGetAsync()
    {
        var validacion = ValidarAdministrador();

        if (validacion != null)
        {
            return validacion;
        }

        bool tiendaCargada = await CargarDatosAsync();

        if (!tiendaCargada)
        {
            return RedirectToPage("/Tiendas");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostRegistrarPedidoAsync()
    {
        var validacion = ValidarAdministrador();

        if (validacion != null)
        {
            return validacion;
        }

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

        if (UsuarioActual == null)
        {
            TituloMensaje = "Usuario no encontrado";
            Mensaje = "No se pudo identificar el usuario actual.";
            TipoMensaje = "danger";

            return RedirectToPage();
        }

        using (dbContext = new punto_de_ventaContext())
        {
            bool tiendaCargada = await CargarTiendaActualAsync(dbContext);

            if (!tiendaCargada || TiendaActual == null)
            {
                return RedirectToPage("/Tiendas");
            }

            if (TiendaActual.Estado != true)
            {
                TituloMensaje = "Tienda inactiva";
                Mensaje = "No se pueden registrar pedidos en una tienda inactiva.";
                TipoMensaje = "warning";

                return RedirectToPage();
            }

            bool proveedorExiste = await dbContext.Proveedor
                .AnyAsync(p => p.ProveedorId == ProveedorId && p.Activo == true);

            if (!proveedorExiste)
            {
                TituloMensaje = "Proveedor no válido";
                Mensaje = "No puedes registrar un pedido con un proveedor inactivo o inexistente.";
                TipoMensaje = "warning";

                return RedirectToPage();
            }

            var productosValidos = await dbContext.Producto
                .Where(p =>
                    ProductoIds.Contains(p.ProductoId) &&
                    p.Activo == true &&
                    p.ProveedorId == ProveedorId)
                .Select(p => p.ProductoId)
                .ToListAsync();

            if (productosValidos.Count != ProductoIds.Distinct().Count())
            {
                TituloMensaje = "Producto no válido";
                Mensaje = "Todos los productos del pedido deben estar activos y pertenecer al proveedor seleccionado.";
                TipoMensaje = "warning";

                return RedirectToPage();
            }

            decimal montoTotal = 0;

            for (int i = 0; i < ProductoIds.Count; i++)
            {
                montoTotal += Cantidades[i] * CostosUnitarios[i];
            }

            using var transaccion = await dbContext.Database.BeginTransactionAsync();

            try
            {
                HistorialPedido pedido = new HistorialPedido()
                {
                    TiendaId = TiendaActual.TiendaId,
                    ProveedorId = ProveedorId,
                    UsuarioId = UsuarioActual.UsuarioId,
                    Fecha = DateTime.Now,
                    MontoTotal = montoTotal,
                    Estado = "Pendiente"
                };

                dbContext.HistorialPedido.Add(pedido);
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

                    dbContext.HistorialPedidoDetalle.Add(detalle);
                }

                await dbContext.SaveChangesAsync();
                await transaccion.CommitAsync();
            }
            catch
            {
                await transaccion.RollbackAsync();

                TituloMensaje = "Pedido no registrado";
                Mensaje = "Ocurrió un error al registrar el pedido.";
                TipoMensaje = "danger";

                return RedirectToPage();
            }
        }

        TituloMensaje = "Pedido registrado";
        Mensaje = "El pedido se registró correctamente.";
        TipoMensaje = "success";

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostReducirStockAsync()
    {
        var validacion = ValidarAdministrador();

        if (validacion != null)
        {
            return validacion;
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
            bool tiendaCargada = await CargarTiendaActualAsync(dbContext);

            if (!tiendaCargada || TiendaActual == null)
            {
                return RedirectToPage("/Tiendas");
            }

            if (TiendaActual.Estado != true)
            {
                TituloMensaje = "Tienda inactiva";
                Mensaje = "No se puede reducir stock en una tienda inactiva.";
                TipoMensaje = "warning";

                return RedirectToPage();
            }

            Inventario? inventario = await dbContext.Inventario
                .FirstOrDefaultAsync(i =>
                    i.InventarioId == InventarioId &&
                    i.TiendaId == TiendaActual.TiendaId);

            if (inventario == null)
            {
                TituloMensaje = "Reducción no registrada";
                Mensaje = "No se encontró el producto en el inventario de esta tienda.";
                TipoMensaje = "danger";

                return RedirectToPage();
            }

            if (Cantidad <= 0)
            {
                TituloMensaje = "Reducción no registrada";
                Mensaje = "La cantidad a reducir debe ser mayor a 0.";
                TipoMensaje = "warning";

                return RedirectToPage();
            }

            string motivoLimpio = Motivo?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(motivoLimpio))
            {
                TituloMensaje = "Reducción no registrada";
                Mensaje = "Debes escribir una razón para la reducción de stock.";
                TipoMensaje = "warning";

                return RedirectToPage();
            }

            if (inventario.Stock < Cantidad)
            {
                TituloMensaje = "Reducción no registrada";
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
                Motivo = motivoLimpio,
                Fecha = DateTime.Now
            };

            dbContext.HistorialMovimiento.Add(nuevoMovimiento);
            await dbContext.SaveChangesAsync();

            TituloMensaje = "Stock actualizado";
            Mensaje = "La reducción de stock se registró correctamente.";
            TipoMensaje = "success";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAumentarStockAsync()
    {
        var validacion = ValidarAdministrador();

        if (validacion != null)
        {
            return validacion;
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
            bool tiendaCargada = await CargarTiendaActualAsync(dbContext);

            if (!tiendaCargada || TiendaActual == null)
            {
                return RedirectToPage("/Tiendas");
            }

            if (TiendaActual.Estado != true)
            {
                TituloMensaje = "Tienda inactiva";
                Mensaje = "No se puede aumentar stock en una tienda inactiva.";
                TipoMensaje = "warning";

                return RedirectToPage();
            }

            Inventario? inventario = await dbContext.Inventario
                .FirstOrDefaultAsync(i =>
                    i.InventarioId == InventarioId &&
                    i.TiendaId == TiendaActual.TiendaId);

            if (inventario == null)
            {
                TituloMensaje = "Aumento no registrado";
                Mensaje = "No se encontró el producto en el inventario de esta tienda.";
                TipoMensaje = "danger";

                return RedirectToPage();
            }

            if (Cantidad <= 0)
            {
                TituloMensaje = "Aumento no registrado";
                Mensaje = "La cantidad a aumentar debe ser mayor a 0.";
                TipoMensaje = "warning";

                return RedirectToPage();
            }

            string motivoLimpio = Motivo?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(motivoLimpio))
            {
                TituloMensaje = "Aumento no registrado";
                Mensaje = "Debes escribir una razón para el aumento de stock.";
                TipoMensaje = "warning";

                return RedirectToPage();
            }

            inventario.Stock += Cantidad;

            HistorialMovimiento nuevoMovimiento = new HistorialMovimiento()
            {
                InventarioId = InventarioId,
                UsuarioId = UsuarioActual.UsuarioId,
                Tipo = "Entrada",
                Cantidad = Cantidad,
                Motivo = motivoLimpio,
                Fecha = DateTime.Now
            };

            dbContext.HistorialMovimiento.Add(nuevoMovimiento);
            await dbContext.SaveChangesAsync();

            TituloMensaje = "Stock actualizado";
            Mensaje = "El aumento de stock se registró correctamente.";
            TipoMensaje = "success";
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

        bool tiendaCargada = await CargarDatosAsync(ParametroDeBusqueda);

        if (!tiendaCargada)
        {
            return RedirectToPage("/Tiendas");
        }

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

    private async Task<bool> CargarTiendaActualAsync(punto_de_ventaContext context)
    {
        var json = HttpContext.Session.GetString("Tienda");

        if (json == null)
        {
            return false;
        }

        var tiendaSesion = JsonSerializer.Deserialize<Tienda>(json);

        if (tiendaSesion == null)
        {
            HttpContext.Session.Remove("Tienda");
            return false;
        }

        TiendaActual = await context.Tienda
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TiendaId == tiendaSesion.TiendaId);

        if (TiendaActual == null)
        {
            HttpContext.Session.Remove("Tienda");
            return false;
        }

        return true;
    }

    private async Task CargarProveedoresAsync(punto_de_ventaContext context)
    {
        Proveedores = await context.Proveedor
            .AsNoTracking()
            .Where(p => p.Activo == true)
            .OrderBy(p => p.Nombre)
            .ToListAsync();
    }

    private async Task CargarProductosAsync(punto_de_ventaContext context)
    {
        Productos = await context.Producto
            .AsNoTracking()
            .Where(p => p.Activo == true)
            .OrderBy(p => p.Nombre)
            .ToListAsync();
    }

    private async Task<bool> CargarDatosAsync(string? busqueda = null)
    {
        using (dbContext = new punto_de_ventaContext())
        {
            bool tiendaCargada = await CargarTiendaActualAsync(dbContext);

            if (!tiendaCargada || TiendaActual == null)
            {
                return false;
            }

            await CargarProveedoresAsync(dbContext);
            await CargarProductosAsync(dbContext);

            string parametro = busqueda?.Trim() ?? "";

            IQueryable<Inventario> consulta = dbContext.Inventario
                .AsNoTracking()
                .Include(i => i.Producto)
                    .ThenInclude(p => p.Categoria)
                .Include(i => i.Producto)
                    .ThenInclude(p => p.Proveedor)
                .Where(i => i.TiendaId == TiendaActual.TiendaId);

            if (!string.IsNullOrWhiteSpace(parametro))
            {
                consulta = consulta.Where(i =>
                    i.Producto.Nombre.Contains(parametro) ||
                    i.Producto.Codigo.Contains(parametro) ||
                    (
                        i.Producto.Categoria != null &&
                        i.Producto.Categoria.Nombre.Contains(parametro)
                    ) ||
                    (
                        i.Producto.Proveedor != null &&
                        i.Producto.Proveedor.Nombre.Contains(parametro)
                    ));
            }

            Inventarios = await consulta
                .OrderBy(i => i.Producto.Nombre)
                .ToListAsync();
        }

        return true;
    }
}
