using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;
using System.Text.Json;

public class HistorialModel : PageModel
{
    public Usuario? UsuarioActual { get; set; }

    private punto_de_ventaContext? dbContext;

    public List<VentaHistorialVM> Ventas { get; set; } = new();

    [BindProperty]
    public string ParametroDeBusqueda { get; set; } = "";

    public async Task<IActionResult> OnGetAsync()
    {
        var validacion = ValidarAdministrador();

        if (validacion != null)
        {
            return validacion;
        }

        await CargarVentasAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostBusquedaAsync()
    {
        var validacion = ValidarAdministrador();

        if (validacion != null)
        {
            return validacion;
        }

        await CargarVentasAsync(ParametroDeBusqueda);

        return Page();
    }

    private async Task CargarVentasAsync(string busqueda = "")
    {
        using (dbContext = new punto_de_ventaContext())
        {
            busqueda = busqueda?.Trim() ?? "";

            IQueryable<Venta> consulta = dbContext.Venta
                .AsNoTracking()
                .Include(v => v.Usuario)
                .Include(v => v.Pago)
                .Include(v => v.DetalleVenta)
                    .ThenInclude(d => d.Producto);

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                consulta = consulta.Where(v =>
                    v.VentaId.ToString().Contains(busqueda) ||
                    (
                        v.Usuario != null &&
                        v.Usuario.Nombre.Contains(busqueda)
                    ) ||
                    dbContext.Tienda.Any(t =>
                        t.TiendaId == v.TiendaId &&
                        t.Nombre.Contains(busqueda)
                    ) ||
                    v.DetalleVenta.Any(d =>
                        d.Producto != null &&
                        (
                            d.Producto.Nombre.Contains(busqueda) ||
                            d.Producto.Codigo.Contains(busqueda)
                        )
                    ));
            }

            var ventasConsulta = await consulta
                .OrderByDescending(v => v.Fecha)
                .ToListAsync();

            var tiendaIds = ventasConsulta
                .Select(v => v.TiendaId)
                .Distinct()
                .ToList();

            var tiendas = await dbContext.Tienda
                .AsNoTracking()
                .Where(t => tiendaIds.Contains(t.TiendaId))
                .ToDictionaryAsync(t => t.TiendaId, t => t.Nombre);

            Ventas = ventasConsulta.Select(v => new VentaHistorialVM
            {
                VentaId = v.VentaId,
                Usuario = v.Usuario?.Nombre ?? "Sin usuario",
                Tienda = tiendas.ContainsKey(v.TiendaId) ? tiendas[v.TiendaId] : "Sin tienda",
                Fecha = v.Fecha,
                Total = v.Total,
                Detalles = v.DetalleVenta.ToList(),
                Pagos = v.Pago.ToList()
            }).ToList();
        }
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
}

public class VentaHistorialVM
{
    public int VentaId { get; set; }

    public string Usuario { get; set; } = "";

    public string Tienda { get; set; } = "";

    public DateTime Fecha { get; set; }

    public decimal Total { get; set; }

    public List<DetalleVenta> Detalles { get; set; } = new();

    public List<Pago> Pagos { get; set; } = new();
}