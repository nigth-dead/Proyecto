using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;
using System.Text.Json;

public class CortesCajaModel : PageModel
{
    private punto_de_ventaContext? dbContext;

    public Usuario? UsuarioActual { get; set; }

    public List<CorteCajaHistorialVM> Cortes { get; set; } = new();

    [BindProperty]
    public string ParametroDeBusqueda { get; set; } = "";

    public async Task<IActionResult> OnGetAsync()
    {
        var validacion = ValidarAdministrador();

        if (validacion != null)
        {
            return validacion;
        }

        await CargarCortesAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostBusquedaAsync()
    {
        var validacion = ValidarAdministrador();

        if (validacion != null)
        {
            return validacion;
        }

        await CargarCortesAsync(ParametroDeBusqueda);

        return Page();
    }

    public async Task<IActionResult> OnPostCompletarAsync(int corteId)
    {
        var validacion = ValidarAdministrador();

        if (validacion != null)
        {
            return validacion;
        }

        using (dbContext = new punto_de_ventaContext())
        {
            var corte = await dbContext.CorteCaja
                .FirstOrDefaultAsync(c => c.CorteId == corteId);

            if (corte == null)
            {
                return RedirectToPage();
            }

            if (corte.Estado == "Completo")
            {
                return RedirectToPage();
            }
            
            if (corte.FechaCierre == null || corte.SaldoFinal == null)
            {
                return RedirectToPage();
            }

            if (corte.SaldoEsperado == null)
            {
                decimal efectivoVendido = await (
                    from p in dbContext.Pago
                    join v in dbContext.Venta on p.VentaId equals v.VentaId
                    where v.CorteId == corte.CorteId && p.Metodo == "Efectivo"
                    select (decimal?)p.Monto
                ).SumAsync() ?? 0;

                corte.SaldoEsperado = corte.SaldoInicial + efectivoVendido;
            }

            corte.Estado = "Completo";

            await dbContext.SaveChangesAsync();
        }

        return RedirectToPage();
    }

    private async Task CargarCortesAsync(string busqueda = "")
    {
        using (dbContext = new punto_de_ventaContext())
        {
            busqueda = busqueda?.Trim() ?? "";

            var cortesConsulta = await dbContext.CorteCaja
                .AsNoTracking()
                .OrderByDescending(c => c.FechaApertura)
                .ToListAsync();

            var tiendaIds = cortesConsulta
                .Select(c => c.TiendaId)
                .Distinct()
                .ToList();

            var usuarioIds = cortesConsulta
                .Select(c => c.UsuarioId)
                .Distinct()
                .ToList();

            var corteIds = cortesConsulta
                .Select(c => c.CorteId)
                .Distinct()
                .ToList();

            var tiendas = await dbContext.Tienda
                .AsNoTracking()
                .Where(t => tiendaIds.Contains(t.TiendaId))
                .ToDictionaryAsync(t => t.TiendaId, t => t.Nombre);

            var usuarios = await dbContext.Usuario
                .AsNoTracking()
                .Where(u => usuarioIds.Contains(u.UsuarioId))
                .ToDictionaryAsync(u => u.UsuarioId, u => u.Nombre);

            var ventasPorCorte = await dbContext.Venta
                .AsNoTracking()
                .Where(v => corteIds.Contains(v.CorteId))
                .GroupBy(v => v.CorteId)
                .Select(g => new
                {
                    CorteId = g.Key,
                    NumeroVentas = g.Count(),
                    TotalVendido = g.Sum(v => v.Total)
                })
                .ToListAsync();

            var pagosPorCorte = await (
                from p in dbContext.Pago.AsNoTracking()
                join v in dbContext.Venta.AsNoTracking() on p.VentaId equals v.VentaId
                where corteIds.Contains(v.CorteId)
                group p by new
                {
                    v.CorteId,
                    p.Metodo
                }
                into grupo
                select new
                {
                    grupo.Key.CorteId,
                    grupo.Key.Metodo,
                    Total = grupo.Sum(p => p.Monto)
                })
                .ToListAsync();

            Cortes = cortesConsulta.Select(c =>
            {
                var ventas = ventasPorCorte.FirstOrDefault(v => v.CorteId == c.CorteId);

                decimal efectivoVendido = pagosPorCorte
                    .Where(p => p.CorteId == c.CorteId && p.Metodo == "Efectivo")
                    .Sum(p => p.Total);

                decimal tarjetaVendido = pagosPorCorte
                    .Where(p => p.CorteId == c.CorteId && p.Metodo == "Tarjeta")
                    .Sum(p => p.Total);

                return new CorteCajaHistorialVM
                {
                    CorteId = c.CorteId,
                    TiendaId = c.TiendaId,
                    UsuarioId = c.UsuarioId,
                    Tienda = tiendas.ContainsKey(c.TiendaId) ? tiendas[c.TiendaId] : "Sin tienda",
                    Usuario = usuarios.ContainsKey(c.UsuarioId) ? usuarios[c.UsuarioId] : "Sin usuario",
                    FechaApertura = c.FechaApertura,
                    FechaCierre = c.FechaCierre,
                    SaldoInicial = c.SaldoInicial,
                    SaldoEsperado = c.SaldoEsperado,
                    SaldoFinal = c.SaldoFinal,
                    Estado = c.Estado,
                    NumeroVentas = ventas?.NumeroVentas ?? 0,
                    TotalVendido = ventas?.TotalVendido ?? 0,
                    EfectivoVendido = efectivoVendido,
                    TarjetaVendido = tarjetaVendido
                };
            }).ToList();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                string texto = busqueda.ToLower();

                Cortes = Cortes.Where(c =>
                    c.CorteId.ToString().Contains(texto) ||
                    c.Tienda.ToLower().Contains(texto) ||
                    c.Usuario.ToLower().Contains(texto) ||
                    c.Estado.ToLower().Contains(texto) ||
                    c.FechaApertura.ToString("dd/MM/yyyy HH:mm").Contains(texto) ||
                    (c.FechaCierre.HasValue && c.FechaCierre.Value.ToString("dd/MM/yyyy HH:mm").Contains(texto)) ||
                    c.SaldoInicial.ToString().Contains(texto) ||
                    (c.SaldoEsperado.HasValue && c.SaldoEsperado.Value.ToString().Contains(texto)) ||
                    (c.SaldoFinal.HasValue && c.SaldoFinal.Value.ToString().Contains(texto))
                ).ToList();
            }
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

        if (UsuarioActual.Rol?.Trim() != "Administrador")
        {
            return RedirectToPage("/Index");
        }

        return null;
    }
}

public class CorteCajaHistorialVM
{
    public int CorteId { get; set; }

    public int TiendaId { get; set; }

    public int UsuarioId { get; set; }

    public string Tienda { get; set; } = "";

    public string Usuario { get; set; } = "";

    public DateTime FechaApertura { get; set; }

    public DateTime? FechaCierre { get; set; }

    public decimal SaldoInicial { get; set; }

    public decimal? SaldoEsperado { get; set; }

    public decimal? SaldoFinal { get; set; }

    public string Estado { get; set; } = "";

    public int NumeroVentas { get; set; }

    public decimal TotalVendido { get; set; }

    public decimal EfectivoVendido { get; set; }

    public decimal TarjetaVendido { get; set; }

    public decimal? Diferencia
    {
        get
        {
            if (!SaldoFinal.HasValue || !SaldoEsperado.HasValue)
            {
                return null;
            }

            return SaldoFinal.Value - SaldoEsperado.Value;
        }
    }
}
