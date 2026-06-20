using Microsoft.AspNetCore.Mvc.RazorPages;
using Proyecto.Models;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace Proyecto.Pages;

public class CajeroModel : PageModel
{
    private readonly punto_de_ventaContext _context;

    public Usuario? UsuarioActual { get; set; }
    public string Fecha { get; set; } = "";
    public decimal TotalCorteActual { get; set; }
    public decimal EfectivoEsperado { get; set; }
    public Tienda? Tienda { get; set; }

    [BindProperty]
    public decimal SaldoFinal { get; set; }

    [BindProperty]
    public decimal SaldoInicial { get; set; }

    public bool CorteActivo { get; set; }

    

    public CajeroModel(punto_de_ventaContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var usuario = await ObtenerUsuarioActualAsync();

        if (usuario == null)
        {
            return RedirectToPage("/Index");
        }

        if (usuario.Rol != "Cajero")
        {
            return RedirectToPage("/Index");
        }

        if (usuario.Contratado != true)
        {
            HttpContext.Session.Remove("Usuario");
            HttpContext.Session.Remove("Tienda");

            return RedirectToPage("/Index");
        }

        UsuarioActual = usuario;

        Tienda = await ObtenerTiendaActivaAsignadaAsync(usuario.UsuarioId);

        if (Tienda == null)
        {
            HttpContext.Session.Remove("Tienda");
            return RedirectToPage("/Index");
        }

        Fecha = DateTime.Now.ToString("dd '/' MM '/' yyyy",
            new System.Globalization.CultureInfo("es-MX"));

        var corte = await _context.CorteCaja
            .FirstOrDefaultAsync(c =>
                c.UsuarioId == usuario.UsuarioId &&
                c.TiendaId == Tienda.TiendaId &&
                c.Estado == "Pendiente" &&
                c.FechaCierre == null);

        if (corte != null)
        {
            CorteActivo = true;

            TotalCorteActual = await _context.Venta
                .Where(v => v.CorteId == corte.CorteId)
                .SumAsync(v => (decimal?)v.Total) ?? 0;

            decimal efectivoVendido = await (
                from p in _context.Pago
                join v in _context.Venta on p.VentaId equals v.VentaId
                where v.CorteId == corte.CorteId && p.Metodo == "Efectivo"
                select (decimal?)p.Monto
            ).SumAsync() ?? 0;

            EfectivoEsperado = corte.SaldoInicial + efectivoVendido;
        }

        return Page();
    }

    public async Task<JsonResult> OnGetBuscarProductosAsync(string termino)
    {
        if (string.IsNullOrWhiteSpace(termino))
        {
            return new JsonResult(new List<object>());
        }

        var usuario = await ObtenerUsuarioActualAsync();

        if (usuario == null || usuario.Rol != "Cajero" || usuario.Contratado != true)
        {
            return new JsonResult(new List<object>());
        }

        var tienda = await ObtenerTiendaActivaAsignadaAsync(usuario.UsuarioId);

        if (tienda == null)
        {
            return new JsonResult(new List<object>());
        }

        var productos = await _context.Inventario
            .AsNoTracking()
            .Include(i => i.Producto)
            .Where(i =>
                i.TiendaId == tienda.TiendaId &&
                i.Producto.Activo == true &&
                i.Producto.PrecioVenta != null &&
                (
                    i.Producto.Nombre.Contains(termino) ||
                    i.Producto.Codigo.Contains(termino)
                ))
            .Select(i => new
            {
                codigo = i.Producto.Codigo,
                nombre = i.Producto.Nombre,
                precio = i.Producto.PrecioVenta ?? 0m,
                stock = i.Stock
            })
            .Take(10)
            .ToListAsync();

        return new JsonResult(productos);
    }

    public async Task<IActionResult> OnPostAbrirCajaAsync()
    {
        var usuario = await ObtenerUsuarioActualAsync();

        if (usuario == null)
        {
            return RedirectToPage("/Index");
        }

        if (usuario.Rol != "Cajero")
        {
            return RedirectToPage("/Index");
        }

        if (usuario.Contratado != true)
        {
            HttpContext.Session.Remove("Usuario");
            HttpContext.Session.Remove("Tienda");

            return RedirectToPage("/Index");
        }

        var tienda = await ObtenerTiendaActivaAsignadaAsync(usuario.UsuarioId);

        if (tienda == null)
        {
            HttpContext.Session.Remove("Tienda");
            return RedirectToPage("/Index");
        }

        var corteExistente = await _context.CorteCaja
            .FirstOrDefaultAsync(c =>
                c.UsuarioId == usuario.UsuarioId &&
                c.TiendaId == tienda.TiendaId &&
                c.Estado == "Pendiente" &&
                c.FechaCierre == null);

        if (corteExistente != null)
        {
            return RedirectToPage();
        }

        if (SaldoInicial < 0)
        {
            return RedirectToPage();
        }

        var corte = new CorteCaja
        {
            TiendaId = tienda.TiendaId,
            UsuarioId = usuario.UsuarioId,
            FechaApertura = DateTime.Now,
            SaldoInicial = SaldoInicial,
            SaldoEsperado = SaldoInicial,
            Estado = "Pendiente"
        };

        _context.CorteCaja.Add(corte);
        await _context.SaveChangesAsync();

        return RedirectToPage();
    }

    public async Task<JsonResult> OnPostCobrarAsync([FromBody] VentaRequest request)
    {
        if (request == null || request.Carrito == null || request.Carrito.Count == 0)
        {
            return new JsonResult(new { exito = false, mensaje = "El carrito está vacío." });
        }

        request.MetodoPago = request.MetodoPago?.Trim() ?? "";

        if (request.MetodoPago != "Efectivo" && request.MetodoPago != "Tarjeta" && request.MetodoPago != "Ambos")
        {
            return new JsonResult(new { exito = false, mensaje = "Método de pago no válido." });
        }

        using var transaccion = await _context.Database.BeginTransactionAsync();

        try
        {
            var usuario = await ObtenerUsuarioActualAsync();

            if (usuario == null)
            {
                await transaccion.RollbackAsync();

                return new JsonResult(new
                {
                    exito = false,
                    mensaje = "No se pudo identificar al usuario."
                });
            }

            if (usuario.Rol != "Cajero")
            {
                await transaccion.RollbackAsync();

                return new JsonResult(new
                {
                    exito = false,
                    mensaje = "No tienes permiso para realizar ventas."
                });
            }

            if (usuario.Contratado != true)
            {
                await transaccion.RollbackAsync();

                return new JsonResult(new
                {
                    exito = false,
                    mensaje = "Este usuario no está contratado."
                });
            }

            var tienda = await ObtenerTiendaActivaAsignadaAsync(usuario.UsuarioId);

            if (tienda == null)
            {
                await transaccion.RollbackAsync();

                return new JsonResult(new
                {
                    exito = false,
                    mensaje = "La tienda está inactiva, no existe o no está asignada a este usuario."
                });
            }

            var corte = await _context.CorteCaja
                .FirstOrDefaultAsync(c =>
                    c.TiendaId == tienda.TiendaId &&
                    c.UsuarioId == usuario.UsuarioId &&
                    c.Estado == "Pendiente" &&
                    c.FechaCierre == null);

            if (corte == null)
            {
                await transaccion.RollbackAsync();

                return new JsonResult(new
                {
                    exito = false,
                    mensaje = "Debes abrir la caja antes de registrar ventas."
                });
            }

            decimal subtotalVenta = 0;
            decimal ivaVenta = 0;
            decimal totalVenta = 0;
            const decimal porcentajeIva = 0.16m;

            var listaDetalles = new List<DetalleVenta>();
            var movimientosInventario = new List<HistorialMovimiento>();

            foreach (var item in request.Carrito)
            {
                if (item.Cantidad <= 0)
                {
                    await transaccion.RollbackAsync();
                    return new JsonResult(new { exito = false, mensaje = "La cantidad debe ser mayor a 0." });
                }

                var inventario = await _context.Inventario
                    .Include(i => i.Producto)
                    .FirstOrDefaultAsync(i =>
                        i.TiendaId == tienda.TiendaId &&
                        i.Producto.Codigo == item.Codigo &&
                        i.Producto.Activo == true);

                if (inventario == null)
                {
                    await transaccion.RollbackAsync();

                    return new JsonResult(new
                    {
                        exito = false,
                        mensaje = $"El producto con código {item.Codigo} no está disponible en esta tienda."
                    });
                }

                if (inventario.Producto.PrecioVenta == null)
                {
                    await transaccion.RollbackAsync();

                    return new JsonResult(new
                    {
                        exito = false,
                        mensaje = $"El producto {inventario.Producto.Nombre} no tiene precio de venta asignado."
                    });
                }

                if (inventario.Stock < item.Cantidad)
                {
                    await transaccion.RollbackAsync();

                    return new JsonResult(new
                    {
                        exito = false,
                        mensaje = $"No hay suficiente stock de {inventario.Producto.Nombre}. Stock disponible: {inventario.Stock}."
                    });
                }

                decimal precioVenta = inventario.Producto.PrecioVenta.Value;

                decimal subtotalProducto = precioVenta * item.Cantidad;
                decimal ivaProducto = subtotalProducto * porcentajeIva;
                decimal totalProducto = subtotalProducto + ivaProducto;

                subtotalVenta += subtotalProducto;
                ivaVenta += ivaProducto;
                totalVenta += totalProducto;

                inventario.Stock -= item.Cantidad;

                listaDetalles.Add(new DetalleVenta
                {
                    ProductoId = inventario.ProductoId,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = precioVenta,
                    Subtotal = subtotalProducto,
                    Iva = ivaProducto
                });

                movimientosInventario.Add(new HistorialMovimiento
                {
                    InventarioId = inventario.InventarioId,
                    UsuarioId = usuario.UsuarioId,
                    Tipo = "Venta",
                    Cantidad = item.Cantidad,
                    Motivo = "Venta realizada",
                    Fecha = DateTime.Now
                });
            }

            decimal montoRecibido = request.MontoRecibido ?? 0;
            decimal montoTarjeta = request.MontoTarjeta ?? 0;
            decimal cambio = 0;

            decimal montoEfectivoParaPago = 0;
            decimal montoTarjetaParaPago = 0;

            if (request.MetodoPago == "Efectivo")
            {
                montoTarjeta = 0;

                if (montoRecibido < totalVenta)
                {
                    await transaccion.RollbackAsync();

                    return new JsonResult(new
                    {
                        exito = false,
                        mensaje = "El dinero recibido no alcanza para pagar la venta."
                    });
                }

                montoEfectivoParaPago = totalVenta;
                cambio = montoRecibido - totalVenta;
            }
            else if (request.MetodoPago == "Tarjeta")
            {
                montoRecibido = 0;
                montoTarjeta = totalVenta;

                montoTarjetaParaPago = totalVenta;
                cambio = 0;
            }
            else if (request.MetodoPago == "Ambos")
            {
                if (montoRecibido <= 0 || montoTarjeta <= 0)
                {
                    await transaccion.RollbackAsync();

                    return new JsonResult(new
                    {
                        exito = false,
                        mensaje = "Debes ingresar monto en efectivo y monto con tarjeta."
                    });
                }

                if (montoTarjeta >= totalVenta)
                {
                    await transaccion.RollbackAsync();

                    return new JsonResult(new
                    {
                        exito = false,
                        mensaje = "Para pagar con ambos métodos, el monto con tarjeta debe ser menor al total."
                    });
                }

                decimal efectivoNecesario = totalVenta - montoTarjeta;

                if (montoRecibido < efectivoNecesario)
                {
                    await transaccion.RollbackAsync();

                    return new JsonResult(new
                    {
                        exito = false,
                        mensaje = "La suma de efectivo y tarjeta no alcanza para pagar la venta."
                    });
                }

                montoEfectivoParaPago = efectivoNecesario;
                montoTarjetaParaPago = montoTarjeta;
                cambio = montoRecibido - efectivoNecesario;
            }

            var nuevaVenta = new Venta
            {
                TiendaId = tienda.TiendaId,
                CorteId = corte.CorteId,
                UsuarioId = usuario.UsuarioId,
                Fecha = DateTime.Now,
                Total = totalVenta,
                DetalleVenta = listaDetalles
            };

            _context.Venta.Add(nuevaVenta);
            await _context.SaveChangesAsync();

            foreach (var movimiento in movimientosInventario)
            {
                movimiento.Motivo = $"Venta {nuevaVenta.VentaId}";
            }

            _context.HistorialMovimiento.AddRange(movimientosInventario);

            var pagos = new List<Pago>();

            if (montoEfectivoParaPago > 0)
            {
                pagos.Add(new Pago
                {
                    VentaId = nuevaVenta.VentaId,
                    Monto = montoEfectivoParaPago,
                    Metodo = "Efectivo"
                });
            }

            if (montoTarjetaParaPago > 0)
            {
                pagos.Add(new Pago
                {
                    VentaId = nuevaVenta.VentaId,
                    Monto = montoTarjetaParaPago,
                    Metodo = "Tarjeta"
                });
            }

            _context.Pago.AddRange(pagos);

            await _context.SaveChangesAsync();

            decimal efectivoCorteActual = await (
                from p in _context.Pago
                join v in _context.Venta on p.VentaId equals v.VentaId
                where v.CorteId == corte.CorteId && p.Metodo == "Efectivo"
                select (decimal?)p.Monto
            ).SumAsync() ?? 0;

            corte.SaldoEsperado = corte.SaldoInicial + efectivoCorteActual;

            await _context.SaveChangesAsync();

            decimal totalCorteActual = await _context.Venta
                .Where(v => v.CorteId == corte.CorteId)
                .SumAsync(v => (decimal?)v.Total) ?? 0;

            decimal? efectivoEsperado = corte.SaldoEsperado;

            await transaccion.CommitAsync();

            return new JsonResult(new
            {
                exito = true,
                mensaje = "Venta guardada correctamente.",
                codigoVenta = $"V-{nuevaVenta.VentaId.ToString("D6")}",
                ventaId = nuevaVenta.VentaId,
                tienda = tienda.Nombre,
                cajero = usuario.Nombre,
                fecha = nuevaVenta.Fecha.ToString("dd/MM/yyyy HH:mm"),
                metodoPago = request.MetodoPago,
                subtotal = subtotalVenta,
                iva = ivaVenta,
                total = totalVenta,
                montoRecibido = montoRecibido,
                montoTarjeta = montoTarjeta,
                cambio = cambio,
                totalCorteActual = totalCorteActual,
                efectivoEsperado = efectivoEsperado
            });
        }
        catch (Exception ex)
        {
            await transaccion.RollbackAsync();

            string errorReal = ex.InnerException != null ? ex.InnerException.Message : ex.Message;

            return new JsonResult(new
            {
                exito = false,
                mensaje = "Error BD: " + errorReal
            });
        }
    }

    public async Task<IActionResult> OnPostCorteCajaAsync()
    {
        var usuario = await ObtenerUsuarioActualAsync();

        if (usuario == null)
        {
            return RedirectToPage("/Index");
        }

        if (usuario.Rol != "Cajero")
        {
            return RedirectToPage("/Index");
        }

        if (usuario.Contratado != true)
        {
            HttpContext.Session.Remove("Usuario");
            HttpContext.Session.Remove("Tienda");

            return RedirectToPage("/Index");
        }

        if (SaldoFinal < 0)
        {
            return RedirectToPage();
        }

        var tienda = await ObtenerTiendaActivaAsignadaAsync(usuario.UsuarioId);

        if (tienda == null)
        {
            HttpContext.Session.Remove("Tienda");
            return RedirectToPage("/Index");
        }

        var corte = await _context.CorteCaja
            .FirstOrDefaultAsync(c =>
                c.UsuarioId == usuario.UsuarioId &&
                c.TiendaId == tienda.TiendaId &&
                c.Estado == "Pendiente" &&
                c.FechaCierre == null);

        if (corte == null)
        {
            return RedirectToPage();
        }

        decimal efectivoVendido = await (
            from p in _context.Pago
            join v in _context.Venta on p.VentaId equals v.VentaId
            where v.CorteId == corte.CorteId && p.Metodo == "Efectivo"
            select (decimal?)p.Monto
        ).SumAsync() ?? 0;

        decimal efectivoEsperado = corte.SaldoInicial + efectivoVendido;

        corte.SaldoEsperado = efectivoEsperado;
        corte.SaldoFinal = SaldoFinal;
        corte.FechaCierre = DateTime.Now;

        if (SaldoFinal < efectivoEsperado)
        {
            corte.Estado = "Pendiente";
        }
        else
        {
            corte.Estado = "Completo";
        }

        usuario.Trabajando = false;

        await _context.SaveChangesAsync();

        HttpContext.Session.Remove("Usuario");
        HttpContext.Session.Remove("Tienda");

        return RedirectToPage("/Index");
    }

    private async Task<Usuario?> ObtenerUsuarioActualAsync()
    {
        var json = HttpContext.Session.GetString("Usuario");

        if (json == null)
        {
            return null;
        }

        var usuarioSesion = JsonSerializer.Deserialize<Usuario>(json);

        if (usuarioSesion == null)
        {
            HttpContext.Session.Remove("Usuario");
            HttpContext.Session.Remove("Tienda");

            return null;
        }

        return await _context.Usuario
            .FirstOrDefaultAsync(u => u.UsuarioId == usuarioSesion.UsuarioId);
    }

    private async Task<Tienda?> ObtenerTiendaActivaAsignadaAsync(int usuarioId)
    {
        var json = HttpContext.Session.GetString("Tienda");

        if (json == null)
        {
            return null;
        }

        Tienda? tiendaSesion;

        try
        {
            tiendaSesion = JsonSerializer.Deserialize<Tienda>(json);
        }
        catch
        {
            HttpContext.Session.Remove("Tienda");
            return null;
        }

        if (tiendaSesion == null || tiendaSesion.TiendaId <= 0)
        {
            HttpContext.Session.Remove("Tienda");
            return null;
        }

        var tienda = await _context.Tienda
            .AsNoTracking()
            .FirstOrDefaultAsync(t =>
                t.TiendaId == tiendaSesion.TiendaId &&
                t.Estado == true);

        if (tienda == null)
        {
            HttpContext.Session.Remove("Tienda");
            return null;
        }

        bool usuarioAsignado = await _context.UsuarioTienda
            .AsNoTracking()
            .AnyAsync(ut =>
                ut.UsuarioId == usuarioId &&
                ut.TiendaId == tienda.TiendaId);

        if (!usuarioAsignado)
        {
            HttpContext.Session.Remove("Tienda");
            return null;
        }

        return tienda;
    }
}

public class VentaRequest
{
    public List<ItemCarrito> Carrito { get; set; } = new();
    public string MetodoPago { get; set; } = "Efectivo";
    public decimal? MontoRecibido { get; set; }
    public decimal? MontoTarjeta { get; set; }
}

public class ItemCarrito
{
    public string Codigo { get; set; } = null!;
    public int Cantidad { get; set; }
}
