using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;

namespace Proyecto.Pages;

public class EmpleadosModel : PageModel
{
    private punto_de_ventaContext? dbContext;

    public Usuario? UsuarioActual { get; set; }

    public Tienda? TiendaActual { get; set; }

    public List<Tienda> Tiendas { get; set; } = new();

    public List<Usuario> Usuarios { get; set; } = new();

    [BindProperty]
    public int UsuarioId { get; set; }

    [BindProperty]
    public string Nombre { get; set; } = "";

    [BindProperty]
    public string Telefono { get; set; } = "";

    [BindProperty]
    public string? Contrasena { get; set; }

    [BindProperty]
    public string Rol { get; set; } = "";

    [BindProperty]
    public bool? Contratado { get; set; }

    [BindProperty]
    public string ParametroDeBusqueda { get; set; } = "";

    [BindProperty]
    public List<int> TiendasAsignadasIds { get; set; } = new();

    [TempData]
    public string? Mensaje { get; set; }

    [TempData]
    public string? TipoMensaje { get; set; }

    [TempData]
    public string? TituloMensaje { get; set; }

    [TempData]
    public string? UsuarioIdDespedir { get; set; }

    [TempData]
    public string? NombreUsuarioDespedir { get; set; }

    [TempData]
    public string? MensajeDespedir { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var validacion = ValidarAdministrador();

        if (validacion != null)
        {
            return validacion;
        }

        using (dbContext = new punto_de_ventaContext())
        {
            bool tiendaValida = await CargarTiendaActualAsync(dbContext);

            if (!tiendaValida)
            {
                return RedirectToPage("/Tiendas");
            }

            await CargarDatosAsync(dbContext);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostRegistrarEmpleadoAsync()
    {
        var validacion = ValidarAdministrador();

        if (validacion != null)
        {
            return validacion;
        }

        using (dbContext = new punto_de_ventaContext())
        {
            bool tiendaValida = await CargarTiendaActualAsync(dbContext);

            if (!tiendaValida || TiendaActual == null)
            {
                TituloMensaje = "Tienda no encontrada";
                Mensaje = "No se pudo identificar la tienda donde se registrará el empleado.";
                TipoMensaje = "danger";

                return RedirectToPage("/Tiendas");
            }

            if (string.IsNullOrWhiteSpace(Nombre))
            {
                TituloMensaje = "Nombre obligatorio";
                Mensaje = "Debes ingresar el nombre del empleado.";
                TipoMensaje = "warning";

                return RedirectToPage();
            }

            Telefono = Telefono.Trim();

            if (string.IsNullOrWhiteSpace(Telefono) || Telefono.Length != 10 || !Telefono.All(char.IsDigit))
            {
                TituloMensaje = "Teléfono inválido";
                Mensaje = "El teléfono debe contener exactamente 10 dígitos numéricos.";
                TipoMensaje = "warning";

                return RedirectToPage();
            }

            if (!ContrasenaSegura(Contrasena))
            {
                TituloMensaje = "Contraseña insegura";
                Mensaje = "La contraseña debe tener al menos 8 caracteres, una mayúscula, una minúscula y un número.";
                TipoMensaje = "warning";

                return RedirectToPage();
            }

            if (Rol != "Administrador" && Rol != "Cajero")
            {
                TituloMensaje = "Rol inválido";
                Mensaje = "Debes seleccionar un rol válido.";
                TipoMensaje = "warning";

                return RedirectToPage();
            }

            Usuario nuevoUsuario = new Usuario()
            {
                Nombre = Nombre.Trim(),
                Telefono = Telefono,
                Contrasena = "",
                Rol = Rol,
                Trabajando = false,
                Contratado = true
            };

            var passwordHasher = new PasswordHasher<Usuario>();
            nuevoUsuario.Contrasena = passwordHasher.HashPassword(nuevoUsuario, Contrasena!);

            dbContext.Usuario.Add(nuevoUsuario);
            await dbContext.SaveChangesAsync();

            await GuardarTiendasAsignadasAsync(
                dbContext,
                nuevoUsuario.UsuarioId,
                TiendaActual.TiendaId
            );

            await dbContext.SaveChangesAsync();

            TituloMensaje = "Empleado registrado";
            Mensaje = "El empleado se registró correctamente.";
            TipoMensaje = "success";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditarEmpleadoAsync()
    {
        var validacion = ValidarAdministrador();

        if (validacion != null)
        {
            return validacion;
        }

        using (dbContext = new punto_de_ventaContext())
        {
            bool tiendaValida = await CargarTiendaActualAsync(dbContext);

            if (!tiendaValida || TiendaActual == null)
            {
                TituloMensaje = "Tienda no encontrada";
                Mensaje = "No se pudo identificar la tienda actual.";
                TipoMensaje = "danger";

                return RedirectToPage("/Tiendas");
            }

            var usuarioActualId = ObtenerUsuarioActualId();

            if (usuarioActualId == UsuarioId && Contratado != true)
            {
                TituloMensaje = "Acción no permitida";
                Mensaje = "No puedes darte de baja a ti mismo.";
                TipoMensaje = "warning";

                return RedirectToPage();
            }

            var editarUsuario = await dbContext.Usuario
                .Include(u => u.UsuarioTienda)
                .FirstOrDefaultAsync(u => u.UsuarioId == UsuarioId);

            if (editarUsuario == null)
            {
                TituloMensaje = "Empleado no encontrado";
                Mensaje = "No se pudo encontrar el empleado que intentas editar.";
                TipoMensaje = "danger";

                return RedirectToPage();
            }

            if (string.IsNullOrWhiteSpace(Nombre))
            {
                TituloMensaje = "Nombre obligatorio";
                Mensaje = "Debes ingresar el nombre del empleado.";
                TipoMensaje = "warning";

                return RedirectToPage();
            }

            Telefono = Telefono.Trim();

            if (string.IsNullOrWhiteSpace(Telefono) || Telefono.Length != 10 || !Telefono.All(char.IsDigit))
            {
                TituloMensaje = "Teléfono inválido";
                Mensaje = "El teléfono debe contener exactamente 10 dígitos numéricos.";
                TipoMensaje = "warning";

                return RedirectToPage();
            }

            if (!string.IsNullOrWhiteSpace(Contrasena) && !ContrasenaSegura(Contrasena))
            {
                TituloMensaje = "Contraseña insegura";
                Mensaje = "La contraseña debe tener al menos 8 caracteres, una mayúscula, una minúscula y un número.";
                TipoMensaje = "warning";

                return RedirectToPage();
            }

            if (Rol != "Administrador" && Rol != "Cajero")
            {
                TituloMensaje = "Rol inválido";
                Mensaje = "Debes seleccionar un rol válido.";
                TipoMensaje = "warning";

                return RedirectToPage();
            }

            editarUsuario.Nombre = Nombre.Trim();
            editarUsuario.Telefono = Telefono;

            if (!string.IsNullOrWhiteSpace(Contrasena))
            {
                var passwordHasher = new PasswordHasher<Usuario>();
                editarUsuario.Contrasena = passwordHasher.HashPassword(editarUsuario, Contrasena);
            }

            editarUsuario.Rol = Rol;
            editarUsuario.Contratado = Contratado;

            if (Contratado != true)
            {
                editarUsuario.Trabajando = false;
            }

            await GuardarTiendasAsignadasAsync(
                dbContext,
                editarUsuario.UsuarioId,
                TiendaActual.TiendaId
            );

            await dbContext.SaveChangesAsync();

            TituloMensaje = "Empleado actualizado";
            Mensaje = "Los datos del empleado se actualizaron correctamente.";
            TipoMensaje = "success";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEliminarEmpleadoAsync(int id)
    {
        var validacion = ValidarAdministrador();

        if (validacion != null)
        {
            return validacion;
        }

        using (dbContext = new punto_de_ventaContext())
        {
            var usuarioActualId = ObtenerUsuarioActualId();

            if (usuarioActualId == id)
            {
                TituloMensaje = "Acción no permitida";
                Mensaje = "No puedes eliminar tu propio usuario.";
                TipoMensaje = "warning";

                return RedirectToPage();
            }

            var usuario = await dbContext.Usuario
                .FirstOrDefaultAsync(u => u.UsuarioId == id);

            if (usuario == null)
            {
                TituloMensaje = "Empleado no encontrado";
                Mensaje = "No se pudo encontrar el empleado que intentas eliminar.";
                TipoMensaje = "danger";

                return RedirectToPage();
            }

            List<string> relaciones = new();

            bool tieneCortes = await dbContext.CorteCaja
                .AnyAsync(c => c.UsuarioId == id);

            bool tienePedidos = await dbContext.HistorialPedido
                .AnyAsync(p => p.UsuarioId == id);

            bool tieneVentas = await dbContext.Venta
                .AnyAsync(v => v.UsuarioId == id);

            bool tieneMovimientos = await dbContext.HistorialMovimiento
                .AnyAsync(m => m.UsuarioId == id);

            if (tieneCortes)
            {
                relaciones.Add("cortes de caja");
            }

            if (tienePedidos)
            {
                relaciones.Add("pedidos");
            }

            if (tieneVentas)
            {
                relaciones.Add("ventas");
            }

            if (tieneMovimientos)
            {
                relaciones.Add("movimientos de inventario");
            }

            if (relaciones.Any())
            {
                UsuarioIdDespedir = usuario.UsuarioId.ToString();
                NombreUsuarioDespedir = usuario.Nombre;
                MensajeDespedir = "El empleado no se puede eliminar porque tiene "
                    + string.Join(", ", relaciones)
                    + " registrados.";

                return RedirectToPage();
            }

            var asignacionesUsuario = await dbContext.UsuarioTienda
                .Where(ut => ut.UsuarioId == id)
                .ToListAsync();

            dbContext.UsuarioTienda.RemoveRange(asignacionesUsuario);
            dbContext.Usuario.Remove(usuario);

            await dbContext.SaveChangesAsync();

            TituloMensaje = "Empleado eliminado";
            Mensaje = "El empleado se eliminó correctamente.";
            TipoMensaje = "success";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDespedirEmpleadoAsync(int id)
    {
        var validacion = ValidarAdministrador();

        if (validacion != null)
        {
            return validacion;
        }

        using (dbContext = new punto_de_ventaContext())
        {
            var usuarioActualId = ObtenerUsuarioActualId();

            if (usuarioActualId == id)
            {
                TituloMensaje = "Acción no permitida";
                Mensaje = "No puedes darte de baja a ti mismo.";
                TipoMensaje = "warning";

                return RedirectToPage();
            }

            var usuario = await dbContext.Usuario.FindAsync(id);

            if (usuario != null)
            {
                usuario.Contratado = false;
                usuario.Trabajando = false;

                await dbContext.SaveChangesAsync();

                TituloMensaje = "Empleado despedido";
                Mensaje = "El empleado no fue eliminado, pero se marcó como despedido correctamente.";
                TipoMensaje = "success";
            }
            else
            {
                TituloMensaje = "Empleado no encontrado";
                Mensaje = "No se pudo encontrar el empleado que intentas despedir.";
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

        using (dbContext = new punto_de_ventaContext())
        {
            bool tiendaValida = await CargarTiendaActualAsync(dbContext);

            if (!tiendaValida)
            {
                return RedirectToPage("/Tiendas");
            }

            await CargarDatosAsync(dbContext, ParametroDeBusqueda);
        }

        return Page();
    }

    private async Task CargarDatosAsync(punto_de_ventaContext context, string busqueda = "")
    {
        Tiendas = await context.Tienda
            .Where(t => t.Estado == true)
            .OrderBy(t => t.Nombre)
            .ToListAsync();

        Usuarios = new();

        if (TiendaActual == null)
        {
            return;
        }

        busqueda = busqueda?.Trim() ?? "";

        IQueryable<Usuario> consulta = context.Usuario
            .Include(u => u.UsuarioTienda)
                .ThenInclude(ut => ut.Tienda)
            .Where(u =>
                u.UsuarioTienda.Any(ut => ut.TiendaId == TiendaActual.TiendaId));

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            consulta = consulta.Where(u =>
                u.Nombre.Contains(busqueda) ||
                u.Telefono.Contains(busqueda) ||
                u.Rol.Contains(busqueda) ||
                u.UsuarioTienda.Any(ut =>
                    ut.Tienda != null &&
                    ut.Tienda.Nombre.Contains(busqueda)));
        }

        Usuarios = await consulta
            .OrderBy(u => u.Nombre)
            .ToListAsync();
    }

    private async Task GuardarTiendasAsignadasAsync(
        punto_de_ventaContext context,
        int usuarioId,
        int tiendaActualId)
    {
        var tiendasSeleccionadas = TiendasAsignadasIds
            .Distinct()
            .ToList();

        if (!tiendasSeleccionadas.Contains(tiendaActualId))
        {
            tiendasSeleccionadas.Add(tiendaActualId);
        }

        var tiendasValidas = await context.Tienda
            .Where(t =>
                tiendasSeleccionadas.Contains(t.TiendaId) &&
                t.Estado == true)
            .Select(t => t.TiendaId)
            .ToListAsync();

        if (!tiendasValidas.Contains(tiendaActualId))
        {
            tiendasValidas.Add(tiendaActualId);
        }

        var asignacionesActuales = await context.UsuarioTienda
            .Where(ut => ut.UsuarioId == usuarioId)
            .ToListAsync();

        var idsActuales = asignacionesActuales
            .Select(ut => ut.TiendaId)
            .ToList();

        var asignacionesAEliminar = asignacionesActuales
            .Where(ut => !tiendasValidas.Contains(ut.TiendaId))
            .ToList();

        var tiendasAAgregar = tiendasValidas
            .Where(tiendaId => !idsActuales.Contains(tiendaId))
            .Distinct()
            .ToList();

        context.UsuarioTienda.RemoveRange(asignacionesAEliminar);

        foreach (var tiendaId in tiendasAAgregar)
        {
            context.UsuarioTienda.Add(new UsuarioTienda
            {
                UsuarioId = usuarioId,
                TiendaId = tiendaId
            });
        }
    }

    private bool ContrasenaSegura(string? contrasena)
    {
        return !string.IsNullOrWhiteSpace(contrasena) &&
               contrasena.Length >= 8 &&
               contrasena.Any(char.IsUpper) &&
               contrasena.Any(char.IsLower) &&
               contrasena.Any(char.IsDigit);
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
            .FirstOrDefaultAsync(t =>
                t.TiendaId == tiendaSesion.TiendaId &&
                t.Estado == true);

        if (TiendaActual == null)
        {
            HttpContext.Session.Remove("Tienda");
            return false;
        }

        return true;
    }

    private int? ObtenerUsuarioActualId()
    {
        var json = HttpContext.Session.GetString("Usuario");

        if (json == null)
        {
            return null;
        }

        var usuarioActual = JsonSerializer.Deserialize<Usuario>(json);

        return usuarioActual?.UsuarioId;
    }
}
