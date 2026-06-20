using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;
using System.Text.Json;

namespace Proyecto.Pages;

public class TiendasModel : PageModel
{
    private punto_de_ventaContext? dbContext { get; set; }

    public Usuario? UsuarioActual { get; set; }

    public List<TiendaVM> Tiendas { get; set; } = new();

    [BindProperty]
    public string Nombre { get; set; } = "";

    [BindProperty]
    public string Direccion { get; set; } = "";

    [BindProperty]
    public bool Estado { get; set; }

    [BindProperty]
    public int TiendaId { get; set; }

    [BindProperty]
    public string ParametroDeBusqueda { get; set; } = "";

    [TempData]
    public string? Mensaje { get; set; }

    [TempData]
    public string? TipoMensaje { get; set; }

    [TempData]
    public string? TituloMensaje { get; set; }

    [TempData]
    public string? TiendaIdDesactivar { get; set; }

    [TempData]
    public string? NombreTiendaDesactivar { get; set; }

    [TempData]
    public string? MensajeDesactivar { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var validacion = ValidarAdministrador();

        if (validacion != null)
        {
            return validacion;
        }

        await CargarTiendasAsync();

        return Page();
    }

    public async Task<IActionResult> OnGetAbrirEmpleadosAsync(int id)
    {
        var validacion = ValidarAdministrador();

        if (validacion != null)
        {
            return validacion;
        }

        bool tiendaGuardada = await GuardarTiendaEnSesionAsync(id);

        if (!tiendaGuardada)
        {
            TituloMensaje = "Tienda no encontrada";
            Mensaje = "No se pudo encontrar la tienda seleccionada.";
            TipoMensaje = "danger";

            return RedirectToPage();
        }

        return RedirectToPage("/Empleados");
    }

    public async Task<IActionResult> OnGetAbrirInventarioAsync(int id)
    {
        var validacion = ValidarAdministrador();

        if (validacion != null)
        {
            return validacion;
        }

        bool tiendaGuardada = await GuardarTiendaEnSesionAsync(id);

        if (!tiendaGuardada)
        {
            TituloMensaje = "Tienda no encontrada";
            Mensaje = "No se pudo encontrar la tienda seleccionada.";
            TipoMensaje = "danger";

            return RedirectToPage();
        }

        return RedirectToPage("/DetallesTienda");
    }

    public async Task<IActionResult> OnPostRegistrarAsync()
    {
        var validacion = ValidarAdministrador();

        if (validacion != null)
        {
            return validacion;
        }

        string nombre = Nombre.Trim();
        string direccion = Direccion.Trim();

        if (!ValidarDatosTienda(nombre, direccion))
        {
            return RedirectToPage();
        }

        using (dbContext = new punto_de_ventaContext())
        {
            bool tiendaRepetida = await dbContext.Tienda
                .AnyAsync(t =>
                    t.Nombre == nombre &&
                    t.Direccion == direccion);

            if (tiendaRepetida)
            {
                TituloMensaje = "Tienda repetida";
                Mensaje = "Ya existe una tienda con ese nombre y dirección.";
                TipoMensaje = "warning";

                return RedirectToPage();
            }

            Tienda nuevaTienda = new Tienda()
            {
                Nombre = nombre,
                Direccion = direccion,
                Estado = true
            };

            dbContext.Tienda.Add(nuevaTienda);
            await dbContext.SaveChangesAsync();

            TituloMensaje = "Tienda registrada";
            Mensaje = "La tienda se registró correctamente.";
            TipoMensaje = "success";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditarAsync()
    {
        var validacion = ValidarAdministrador();

        if (validacion != null)
        {
            return validacion;
        }

        string nombre = Nombre.Trim();
        string direccion = Direccion.Trim();

        if (!ValidarDatosTienda(nombre, direccion))
        {
            return RedirectToPage();
        }

        using (dbContext = new punto_de_ventaContext())
        {
            Tienda? tienda = await dbContext.Tienda
                .FirstOrDefaultAsync(t => t.TiendaId == TiendaId);

            if (tienda == null)
            {
                TituloMensaje = "Tienda no encontrada";
                Mensaje = "No se pudo encontrar la tienda que intentas editar.";
                TipoMensaje = "danger";

                return RedirectToPage();
            }

            bool tiendaRepetida = await dbContext.Tienda
                .AnyAsync(t =>
                    t.TiendaId != TiendaId &&
                    t.Nombre == nombre &&
                    t.Direccion == direccion);

            if (tiendaRepetida)
            {
                TituloMensaje = "Tienda repetida";
                Mensaje = "Ya existe otra tienda con ese nombre y dirección.";
                TipoMensaje = "warning";

                return RedirectToPage();
            }

            tienda.Nombre = nombre;
            tienda.Direccion = direccion;
            tienda.Estado = Estado;

            await dbContext.SaveChangesAsync();

            ActualizarTiendaSesionSiEsActual(tienda);

            TituloMensaje = "Tienda actualizada";
            Mensaje = "Los datos de la tienda se actualizaron correctamente.";
            TipoMensaje = "success";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEliminarAsync(int id)
    {
        var validacion = ValidarAdministrador();

        if (validacion != null)
        {
            return validacion;
        }

        using (dbContext = new punto_de_ventaContext())
        {
            var tienda = await dbContext.Tienda
                .FirstOrDefaultAsync(t => t.TiendaId == id);

            if (tienda == null)
            {
                TituloMensaje = "Tienda no encontrada";
                Mensaje = "No se pudo encontrar la tienda que intentas eliminar.";
                TipoMensaje = "danger";

                return RedirectToPage();
            }

            List<string> relaciones = new();

            bool tieneEmpleadosAsignados = await dbContext.UsuarioTienda
                .AnyAsync(ut => ut.TiendaId == id);

            bool tienePedidos = await dbContext.HistorialPedido
                .AnyAsync(p => p.TiendaId == id);

            bool tieneCortes = await dbContext.CorteCaja
                .AnyAsync(c => c.TiendaId == id);

            bool tieneInventario = await dbContext.Inventario
                .AnyAsync(i => i.TiendaId == id);

            bool tieneVentas = await dbContext.Venta
                .AnyAsync(v => v.TiendaId == id);

            if (tieneEmpleadosAsignados)
            {
                relaciones.Add("empleados asignados");
            }

            if (tienePedidos)
            {
                relaciones.Add("pedidos");
            }

            if (tieneCortes)
            {
                relaciones.Add("cortes de caja");
            }

            if (tieneInventario)
            {
                relaciones.Add("inventario");
            }

            if (tieneVentas)
            {
                relaciones.Add("ventas");
            }

            if (relaciones.Any())
            {
                TiendaIdDesactivar = tienda.TiendaId.ToString();
                NombreTiendaDesactivar = tienda.Nombre;
                MensajeDesactivar = "La tienda no se puede eliminar porque tiene "
                    + string.Join(", ", relaciones)
                    + " registrados.";

                return RedirectToPage();
            }

            dbContext.Tienda.Remove(tienda);
            await dbContext.SaveChangesAsync();

            LimpiarTiendaSesionSiEsActual(id);

            TituloMensaje = "Tienda eliminada";
            Mensaje = "La tienda se eliminó correctamente.";
            TipoMensaje = "success";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDesactivarAsync(int id)
    {
        var validacion = ValidarAdministrador();

        if (validacion != null)
        {
            return validacion;
        }

        using (dbContext = new punto_de_ventaContext())
        {
            var tienda = await dbContext.Tienda
                .FirstOrDefaultAsync(t => t.TiendaId == id);

            if (tienda == null)
            {
                TituloMensaje = "Tienda no encontrada";
                Mensaje = "No se pudo encontrar la tienda que intentas desactivar.";
                TipoMensaje = "danger";

                return RedirectToPage();
            }

            if (tienda.Estado == false)
            {
                TituloMensaje = "Tienda ya inactiva";
                Mensaje = "La tienda ya se encontraba desactivada.";
                TipoMensaje = "info";

                return RedirectToPage();
            }

            tienda.Estado = false;

            await dbContext.SaveChangesAsync();

            ActualizarTiendaSesionSiEsActual(tienda);

            TituloMensaje = "Tienda desactivada";
            Mensaje = "La tienda no fue eliminada, pero se desactivó correctamente.";
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

        await CargarTiendasAsync(ParametroDeBusqueda);

        return Page();
    }

    private async Task CargarTiendasAsync(string busqueda = "")
    {
        using (dbContext = new punto_de_ventaContext())
        {
            string parametro = busqueda?.Trim() ?? "";

            IQueryable<Tienda> consulta = dbContext.Tienda
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(parametro))
            {
                consulta = consulta.Where(t =>
                    t.Nombre.Contains(parametro) ||
                    t.Direccion.Contains(parametro));
            }

            var tiendas = await consulta
                .OrderBy(t => t.Nombre)
                .ToListAsync();

            await CalcularTotalesAsync(tiendas);
        }
    }

    private async Task CalcularTotalesAsync(List<Tienda> tiendas)
    {
        using (dbContext = new punto_de_ventaContext())
        {
            var totales = await dbContext.Venta
                .AsNoTracking()
                .Where(v =>
                    v.Fecha >= DateTime.Today &&
                    v.Fecha < DateTime.Today.AddDays(1))
                .GroupBy(v => v.TiendaId)
                .Select(t => new
                {
                    TiendaId = t.Key,
                    Total = t.Sum(v => v.Total)
                })
                .ToListAsync();

            var ganancias = await dbContext.DetalleVenta
                .AsNoTracking()
                .Include(d => d.Venta)
                .Include(d => d.Producto)
                .Where(d =>
                    d.Venta.Fecha >= DateTime.Today &&
                    d.Venta.Fecha < DateTime.Today.AddDays(1))
                .GroupBy(d => d.Venta.TiendaId)
                .Select(g => new
                {
                    TiendaId = g.Key,
                    Ganancia = g.Sum(d =>
                        (d.PrecioUnitario - d.Producto.PrecioCompra) * d.Cantidad)
                })
                .ToListAsync();

            Tiendas = tiendas.Select(t =>
            {
                var total = totales.FirstOrDefault(x => x.TiendaId == t.TiendaId);
                var ganancia = ganancias.FirstOrDefault(x => x.TiendaId == t.TiendaId);

                return new TiendaVM
                {
                    TiendaId = t.TiendaId,
                    Nombre = t.Nombre,
                    Direccion = t.Direccion,
                    Estado = t.Estado,
                    VentaTotal = total?.Total ?? 0,
                    GananciaTotal = ganancia?.Ganancia ?? 0
                };
            }).ToList();
        }
    }

    private async Task<bool> GuardarTiendaEnSesionAsync(int tiendaId)
    {
        using (dbContext = new punto_de_ventaContext())
        {
            var tienda = await dbContext.Tienda
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TiendaId == tiendaId);

            if (tienda == null)
            {
                return false;
            }

            Tienda tiendaSesion = new Tienda()
            {
                TiendaId = tienda.TiendaId,
                Nombre = tienda.Nombre,
                Direccion = tienda.Direccion,
                Estado = tienda.Estado
            };

            HttpContext.Session.SetString(
                "Tienda",
                JsonSerializer.Serialize(tiendaSesion)
            );

            return true;
        }
    }

    private bool ValidarDatosTienda(string nombre, string direccion)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            TituloMensaje = "Nombre inválido";
            Mensaje = "El nombre de la tienda no puede estar vacío.";
            TipoMensaje = "warning";

            return false;
        }

        if (nombre.Length > 50)
        {
            TituloMensaje = "Nombre demasiado largo";
            Mensaje = "El nombre de la tienda no puede superar los 50 caracteres.";
            TipoMensaje = "warning";

            return false;
        }

        if (string.IsNullOrWhiteSpace(direccion))
        {
            TituloMensaje = "Dirección inválida";
            Mensaje = "La dirección de la tienda no puede estar vacía.";
            TipoMensaje = "warning";

            return false;
        }

        if (direccion.Length > 50)
        {
            TituloMensaje = "Dirección demasiado larga";
            Mensaje = "La dirección de la tienda no puede superar los 50 caracteres.";
            TipoMensaje = "warning";

            return false;
        }

        return true;
    }

    private void ActualizarTiendaSesionSiEsActual(Tienda tienda)
    {
        var json = HttpContext.Session.GetString("Tienda");

        if (json == null)
        {
            return;
        }

        var tiendaSesionActual = JsonSerializer.Deserialize<Tienda>(json);

        if (tiendaSesionActual == null || tiendaSesionActual.TiendaId != tienda.TiendaId)
        {
            return;
        }

        Tienda tiendaSesion = new Tienda()
        {
            TiendaId = tienda.TiendaId,
            Nombre = tienda.Nombre,
            Direccion = tienda.Direccion,
            Estado = tienda.Estado
        };

        HttpContext.Session.SetString(
            "Tienda",
            JsonSerializer.Serialize(tiendaSesion)
        );
    }

    private void LimpiarTiendaSesionSiEsActual(int tiendaId)
    {
        var json = HttpContext.Session.GetString("Tienda");

        if (json == null)
        {
            return;
        }

        var tiendaSesionActual = JsonSerializer.Deserialize<Tienda>(json);

        if (tiendaSesionActual != null && tiendaSesionActual.TiendaId == tiendaId)
        {
            HttpContext.Session.Remove("Tienda");
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

public class TiendaVM
{
    public int TiendaId { get; set; }

    public string Nombre { get; set; } = "";

    public string Direccion { get; set; } = "";

    public bool? Estado { get; set; }

    public decimal VentaTotal { get; set; }

    public decimal GananciaTotal { get; set; }
}