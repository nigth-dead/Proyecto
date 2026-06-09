using Microsoft.AspNetCore.Mvc.RazorPages;
using Proyecto.Models;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Authorization;

namespace Proyecto.Pages;

[AllowAnonymous]

public class CajeroModel : PageModel
{
    private readonly punto_de_ventaContext _context;

    public Usuario? UsuarioActual { get; set; }
    public string Fecha { get; set; } = "";

    public CajeroModel(punto_de_ventaContext context)
    {
        _context = context;
    }

    public void OnGet()
    {
        /*Informacion de sesion*/
        var Json = HttpContext.Session.GetString("Usuario");

        if (Json != null)
        {
            UsuarioActual =
            JsonSerializer.Deserialize<Usuario>(Json);
        }

        Fecha = DateTime.Now.ToString("dd '/' MM '/' yyyy",
            new System.Globalization.CultureInfo("es-MX"));
    }
    public async Task<JsonResult> OnGetBuscarProductosAsync(string termino)
    {
        if (string.IsNullOrWhiteSpace(termino) || termino.Length < 2)
        {
            return new JsonResult(new List<object>());
        }

        var productos = await _context.Producto
            .Where(p => p.Nombre.Contains(termino) || p.Codigo.Contains(termino))
            .Select(p => new
            {
                codigo = p.Codigo,
                nombre = p.Nombre,
                precio = p.Precio
            })
            .Take(10)
            .ToListAsync();

        return new JsonResult(productos);
    }

    public async Task<JsonResult> OnPostCobrarAsync([FromBody] List<ItemCarrito> carrito)
    {
        if (carrito == null || carrito.Count == 0)
        {
            return new JsonResult(new { exito = false, mensaje = "El carrito está vacío." });
        }

        try
        {
            // 1. Verificamos las dependencias (Mocking)
            var tienda = await _context.Tienda.FirstOrDefaultAsync() ?? new Tienda { Nombre = "Tienda Principal", Direccion = "Xalapa", Estado = true };
            if (tienda.TiendaId == 0) { _context.Tienda.Add(tienda); await _context.SaveChangesAsync(); }

            var usuario = await _context.Usuario.FirstOrDefaultAsync() ?? new Usuario { TiendaId = tienda.TiendaId, Nombre = "Cajero", Contrasena = "123", Rol = "Cajero" };
            if (usuario.UsuarioId == 0) { _context.Usuario.Add(usuario); await _context.SaveChangesAsync(); }

            var corte = await _context.CorteCaja.FirstOrDefaultAsync(c => c.Estado == "Pendiente") ?? new CorteCaja { TiendaId = tienda.TiendaId, UsuarioId = usuario.UsuarioId, FechaApertura = DateTime.Now, SaldoInicial = 0, Estado = "Pendiente" };
            if (corte.CorteId == 0) { _context.CorteCaja.Add(corte); await _context.SaveChangesAsync(); }

            // 2. Preparamos la lista de detalles y sumamos el total
            decimal totalVenta = 0;
            var listaDetalles = new List<DetalleVenta>();

            foreach (var item in carrito)
            {
                var producto = await _context.Producto.FirstOrDefaultAsync(p => p.Codigo == item.Codigo);
                if (producto != null)
                {
                    decimal subtotal = producto.Precio * item.Cantidad;
                    totalVenta += subtotal;

                    listaDetalles.Add(new DetalleVenta
                    {
                        ProductoId = producto.ProductoId,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = producto.Precio,
                        Iva = 0 
                    });
                }
            }

            // 3. Empaquetamos toda la Venta en un solo objeto
            var nuevaVenta = new Venta
            {
                TiendaId = tienda.TiendaId,
                CorteId = corte.CorteId,
                UsuarioId = usuario.UsuarioId,
                Fecha = DateTime.Now,
                Total = totalVenta,
                DetalleVenta = listaDetalles // <-- C# enlazará los IDs automáticamente aquí
            };

            // 4. Guardamos TODO en un solo movimiento
            _context.Venta.Add(nuevaVenta);
            await _context.SaveChangesAsync(); 

            return new JsonResult(new { exito = true, mensaje = "¡Venta guardada con éxito en la Base de Datos!" });
        }
        catch (Exception ex)
        {
            // EXTRAEMOS EL ERROR PROFUNDO (InnerException)
            string errorReal = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            return new JsonResult(new { exito = false, mensaje = "Error BD: " + errorReal });
        }
    }
}
public class ItemCarrito
{
    public string Codigo { get; set; }
    public int Cantidad { get; set; }
}