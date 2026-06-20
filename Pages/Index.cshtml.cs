using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;

public class IndexModel : PageModel
{
    private readonly punto_de_ventaContext _context;

    [BindProperty]
    public string Nombre { get; set; } = "";

    [BindProperty]
    public string Contrasena { get; set; } = "";

    [BindProperty]
    public int TiendaIdSeleccionada { get; set; }

    public string Mensaje { get; set; } = "";

    public bool MostrarSeleccionTienda { get; set; }

    public List<Tienda> TiendasAsignadas { get; set; } = new();

    public IndexModel(punto_de_ventaContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Nombre = Nombre.Trim();

        if (string.IsNullOrWhiteSpace(Nombre) || string.IsNullOrWhiteSpace(Contrasena))
        {
            Mensaje = "Ingresa usuario y contraseña.";
            return Page();
        }

        var usuarios = await _context.Usuario
            .Where(u => u.Nombre == Nombre)
            .ToListAsync();

        Usuario? usuario = null;

        var passwordHasher = new PasswordHasher<Usuario>();

        foreach (var usuarioEncontrado in usuarios)
        {
            bool contrasenaCorrecta = false;

            if (!string.IsNullOrWhiteSpace(usuarioEncontrado.Contrasena) &&
                usuarioEncontrado.Contrasena.StartsWith("AQAAAA"))
            {
                var resultado = passwordHasher.VerifyHashedPassword(
                    usuarioEncontrado,
                    usuarioEncontrado.Contrasena,
                    Contrasena
                );

                contrasenaCorrecta =
                    resultado == PasswordVerificationResult.Success ||
                    resultado == PasswordVerificationResult.SuccessRehashNeeded;

                if (resultado == PasswordVerificationResult.SuccessRehashNeeded)
                {
                    usuarioEncontrado.Contrasena = passwordHasher.HashPassword(usuarioEncontrado, Contrasena);
                }
            }
            else
            {
                contrasenaCorrecta = usuarioEncontrado.Contrasena == Contrasena;

                if (contrasenaCorrecta)
                {
                    usuarioEncontrado.Contrasena = passwordHasher.HashPassword(usuarioEncontrado, Contrasena);
                }
            }

            if (contrasenaCorrecta)
            {
                usuario = usuarioEncontrado;
                break;
            }
        }

        if (usuario == null)
        {
            Mensaje = "Usuario o contraseña incorrectos.";
            return Page();
        }

        if (usuario.Contratado != true)
        {
            Mensaje = "Este usuario fue dado de baja y no puede iniciar sesión.";
            return Page();
        }

        if (usuario.Rol == "Administrador")
        {
            usuario.Trabajando = true;
            await _context.SaveChangesAsync();

            HttpContext.Session.Remove("UsuarioPendienteId");
            HttpContext.Session.Remove("UsuarioPendiente");
            HttpContext.Session.Remove("Tienda");

            GuardarUsuarioEnSesion(usuario);

            return RedirectToPage("/Administrador");
        }

        if (usuario.Rol == "Cajero")
        {
            TiendasAsignadas = await CargarTiendasActivasAsignadasAsync(usuario.UsuarioId);

            if (!TiendasAsignadas.Any())
            {
                Mensaje = "Este usuario no tiene tiendas activas asignadas.";
                return Page();
            }

            if (TiendasAsignadas.Count == 1)
            {
                usuario.Trabajando = true;
                await _context.SaveChangesAsync();

                HttpContext.Session.Remove("UsuarioPendienteId");
                HttpContext.Session.Remove("UsuarioPendiente");

                GuardarUsuarioEnSesion(usuario);
                GuardarTiendaEnSesion(TiendasAsignadas.First());

                return RedirectToPage("/Cajero");
            }

            HttpContext.Session.Remove("Usuario");
            HttpContext.Session.Remove("Tienda");
            HttpContext.Session.Remove("UsuarioPendiente");
            HttpContext.Session.SetInt32("UsuarioPendienteId", usuario.UsuarioId);

            MostrarSeleccionTienda = true;
            Mensaje = "Selecciona la tienda con la que vas a trabajar.";

            return Page();
        }

        Mensaje = "Rol de usuario no válido.";
        return Page();
    }

    public async Task<IActionResult> OnPostSeleccionarTiendaAsync()
    {
        int? usuarioPendienteId = HttpContext.Session.GetInt32("UsuarioPendienteId");

        if (usuarioPendienteId == null)
        {
            Mensaje = "La sesión expiró. Inicia sesión nuevamente.";
            MostrarSeleccionTienda = false;

            return Page();
        }

        if (TiendaIdSeleccionada <= 0)
        {
            TiendasAsignadas = await CargarTiendasActivasAsignadasAsync(usuarioPendienteId.Value);

            Mensaje = "Selecciona una tienda para continuar.";
            MostrarSeleccionTienda = true;

            return Page();
        }

        var usuario = await _context.Usuario
            .FirstOrDefaultAsync(u => u.UsuarioId == usuarioPendienteId.Value);

        if (usuario == null)
        {
            HttpContext.Session.Remove("UsuarioPendienteId");
            HttpContext.Session.Remove("UsuarioPendiente");

            Mensaje = "No se encontró el usuario.";
            MostrarSeleccionTienda = false;

            return Page();
        }

        if (usuario.Contratado != true)
        {
            HttpContext.Session.Remove("UsuarioPendienteId");
            HttpContext.Session.Remove("UsuarioPendiente");

            Mensaje = "Este usuario fue dado de baja y no puede iniciar sesión.";
            MostrarSeleccionTienda = false;

            return Page();
        }

        if (usuario.Rol != "Cajero")
        {
            HttpContext.Session.Remove("UsuarioPendienteId");
            HttpContext.Session.Remove("UsuarioPendiente");

            Mensaje = "Solo los cajeros pueden seleccionar tienda desde esta pantalla.";
            MostrarSeleccionTienda = false;

            return Page();
        }

        var tienda = await _context.UsuarioTienda
            .AsNoTracking()
            .Where(ut =>
                ut.UsuarioId == usuario.UsuarioId &&
                ut.TiendaId == TiendaIdSeleccionada &&
                ut.Tienda.Estado == true)
            .Select(ut => ut.Tienda)
            .FirstOrDefaultAsync();

        if (tienda == null)
        {
            TiendasAsignadas = await CargarTiendasActivasAsignadasAsync(usuario.UsuarioId);

            Mensaje = "La tienda seleccionada no existe, está inactiva o no está asignada a este usuario.";
            MostrarSeleccionTienda = true;

            return Page();
        }

        usuario.Trabajando = true;
        await _context.SaveChangesAsync();

        HttpContext.Session.Remove("UsuarioPendienteId");
        HttpContext.Session.Remove("UsuarioPendiente");

        GuardarUsuarioEnSesion(usuario);
        GuardarTiendaEnSesion(tienda);

        return RedirectToPage("/Cajero");
    }

    public async Task<IActionResult> OnGetCerrarSesionAsync()
    {
        var json = HttpContext.Session.GetString("Usuario");

        if (json != null)
        {
            var usuarioSesion = JsonSerializer.Deserialize<Usuario>(json);

            if (usuarioSesion != null)
            {
                var usuario = await _context.Usuario
                    .FirstOrDefaultAsync(u => u.UsuarioId == usuarioSesion.UsuarioId);

                if (usuario != null)
                {
                    usuario.Trabajando = false;
                    await _context.SaveChangesAsync();
                }
            }
        }

        HttpContext.Session.Remove("Usuario");
        HttpContext.Session.Remove("UsuarioPendiente");
        HttpContext.Session.Remove("UsuarioPendienteId");
        HttpContext.Session.Remove("Tienda");

        return RedirectToPage("/Index");
    }

    private async Task<List<Tienda>> CargarTiendasActivasAsignadasAsync(int usuarioId)
    {
        return await _context.UsuarioTienda
            .AsNoTracking()
            .Where(ut =>
                ut.UsuarioId == usuarioId &&
                ut.Tienda.Estado == true)
            .Select(ut => ut.Tienda)
            .OrderBy(t => t.Nombre)
            .ToListAsync();
    }

    private void GuardarUsuarioEnSesion(Usuario usuario)
    {
        Usuario usuarioSesion = new Usuario()
        {
            UsuarioId = usuario.UsuarioId,
            Nombre = usuario.Nombre,
            Telefono = usuario.Telefono,
            Contrasena = "",
            Rol = usuario.Rol,
            Trabajando = usuario.Trabajando,
            Contratado = usuario.Contratado
        };

        HttpContext.Session.SetString(
            "Usuario",
            JsonSerializer.Serialize(usuarioSesion)
        );
    }

    private void GuardarTiendaEnSesion(Tienda tienda)
    {
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
}
