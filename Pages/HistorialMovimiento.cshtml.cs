using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;
using System.Text.Json;

namespace Proyecto.Pages;

public class HistorialMovimientoModel : PageModel
{
    private punto_de_ventaContext? dbContext;

    public Usuario? UsuarioActual { get; set; }

    public List<HistorialMovimiento> Movimientos { get; set; } = new();

    [BindProperty]
    public string ParametroDeBusqueda { get; set; } = "";

    [BindProperty(SupportsGet = true)]
    public int? TiendaId { get; set; }

    public string? NombreTiendaFiltro { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var validacion = ValidarAdministrador();

        if (validacion != null)
        {
            return validacion;
        }

        await CargarMovimientosAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostBusquedaAsync()
    {
        var validacion = ValidarAdministrador();

        if (validacion != null)
        {
            return validacion;
        }

        await CargarMovimientosAsync(ParametroDeBusqueda);

        return Page();
    }

    private async Task CargarMovimientosAsync(string busqueda = "")
    {
        using (dbContext = new punto_de_ventaContext())
        {
            await CargarNombreTiendaFiltroAsync();

            IQueryable<HistorialMovimiento> consulta = dbContext.HistorialMovimiento
                .AsNoTracking()
                .Include(m => m.Inventario)
                    .ThenInclude(i => i.Producto)
                .Include(m => m.Inventario)
                    .ThenInclude(i => i.Tienda)
                .Include(m => m.Usuario);

            if (TiendaId.HasValue)
            {
                int tiendaIdFiltro = TiendaId.Value;

                consulta = consulta.Where(m =>
                    m.Inventario != null &&
                    m.Inventario.TiendaId == tiendaIdFiltro);
            }

            var movimientosConsulta = await consulta
                .OrderByDescending(m => m.Fecha)
                .ToListAsync();

            busqueda = busqueda?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(busqueda))
            {
                Movimientos = movimientosConsulta;
                return;
            }

            Movimientos = movimientosConsulta
                .Where(m =>
                    Contiene(m.Tipo, busqueda) ||
                    Contiene(m.Motivo, busqueda) ||
                    Contiene(m.Cantidad.ToString(), busqueda) ||
                    Contiene(m.Fecha.ToString("dd/MM/yyyy HH:mm"), busqueda) ||
                    Contiene(m.Fecha.ToString("yyyy-MM-dd HH:mm:ss"), busqueda) ||
                    Contiene(m.Usuario?.Nombre, busqueda) ||
                    Contiene(m.Inventario?.Tienda?.Nombre, busqueda) ||
                    Contiene(m.Inventario?.Producto?.Nombre, busqueda) ||
                    Contiene(m.Inventario?.Producto?.Codigo, busqueda))
                .ToList();
        }
    }

    private async Task CargarNombreTiendaFiltroAsync()
    {
        NombreTiendaFiltro = null;

        if (dbContext == null || !TiendaId.HasValue)
        {
            return;
        }

        int tiendaIdFiltro = TiendaId.Value;

        NombreTiendaFiltro = await dbContext.Tienda
            .AsNoTracking()
            .Where(t => t.TiendaId == tiendaIdFiltro)
            .Select(t => t.Nombre)
            .FirstOrDefaultAsync();

        if (NombreTiendaFiltro == null)
        {
            NombreTiendaFiltro = "Tienda no encontrada";
        }
    }

    private static bool Contiene(string? texto, string busqueda)
    {
        return !string.IsNullOrWhiteSpace(texto) &&
               texto.Contains(busqueda, StringComparison.OrdinalIgnoreCase);
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