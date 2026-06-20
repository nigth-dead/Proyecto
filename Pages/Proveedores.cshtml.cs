using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using Proyecto.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Proyecto.Pages;

public class ProveedoresModel : PageModel
{
    private punto_de_ventaContext? dbContext;

    public Usuario? UsuarioActual { get; set; }
    public List<Proveedor> Proveedores { get; set; } = new();

    [BindProperty]
    public int ProveedorId { get; set; }

    [BindProperty]
    public string Nombre { get; set; } = "";

    [BindProperty]
    public string Telefono { get; set; } = "";

    [BindProperty]
    public string Correo { get; set; } = "";

    [BindProperty]
    public bool Activo { get; set; }

    [BindProperty]
    public string ParametroDeBusqueda { get; set; } = "";

    [TempData]
    public string? Mensaje { get; set; }

    [TempData]
    public string? TipoMensaje { get; set; }

    [TempData]
    public string? TituloMensaje { get; set; }

    [TempData]
    public string? ProveedorIdDesactivar { get; set; }

    [TempData]
    public string? NombreProveedorDesactivar { get; set; }

    [TempData]
    public string? MensajeDesactivar { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var validacion = ValidarAdministrador();

        if (validacion != null)
        {
            return validacion;
        }

        await CargarProveedoresAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostAgregarAsync()
    {
        var validacion = ValidarAdministrador();

        if (validacion != null)
        {
            return validacion;
        }

        string nombre = Nombre.Trim();
        string telefono = Telefono.Trim();
        string correo = Correo.Trim();

        if (!ValidarDatosProveedor(nombre, telefono, correo))
        {
            return RedirectToPage();
        }

        using (dbContext = new punto_de_ventaContext())
        {
            string nombreNormalizado = nombre.ToLower();
            string correoNormalizado = correo.ToLower();

            bool nombreRepetido = await dbContext.Proveedor
                .AnyAsync(p => p.Nombre.ToLower() == nombreNormalizado);

            if (nombreRepetido)
            {
                TituloMensaje = "Proveedor repetido";
                Mensaje = "Ya existe un proveedor con ese nombre.";
                TipoMensaje = "warning";

                return RedirectToPage();
            }

            bool correoRepetido = await dbContext.Proveedor
                .AnyAsync(p => p.Correo.ToLower() == correoNormalizado);

            if (correoRepetido)
            {
                TituloMensaje = "Correo repetido";
                Mensaje = "Ya existe un proveedor registrado con ese correo.";
                TipoMensaje = "warning";

                return RedirectToPage();
            }

            Proveedor nuevoProveedor = new Proveedor
            {
                Nombre = nombre,
                Telefono = telefono,
                Correo = correo,
                Activo = true
            };

            dbContext.Proveedor.Add(nuevoProveedor);
            await dbContext.SaveChangesAsync();

            TituloMensaje = "Proveedor registrado";
            Mensaje = "El proveedor se registró correctamente.";
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
        string telefono = Telefono.Trim();
        string correo = Correo.Trim();

        if (!ValidarDatosProveedor(nombre, telefono, correo))
        {
            return RedirectToPage();
        }

        using (dbContext = new punto_de_ventaContext())
        {
            var proveedor = await dbContext.Proveedor
                .FirstOrDefaultAsync(p => p.ProveedorId == ProveedorId);

            if (proveedor == null)
            {
                TituloMensaje = "Proveedor no encontrado";
                Mensaje = "No se pudo encontrar el proveedor que intentas editar.";
                TipoMensaje = "danger";

                return RedirectToPage();
            }

            string nombreNormalizado = nombre.ToLower();
            string correoNormalizado = correo.ToLower();

            bool nombreRepetido = await dbContext.Proveedor
                .AnyAsync(p =>
                    p.ProveedorId != ProveedorId &&
                    p.Nombre.ToLower() == nombreNormalizado);

            if (nombreRepetido)
            {
                TituloMensaje = "Proveedor repetido";
                Mensaje = "Ya existe otro proveedor con ese nombre.";
                TipoMensaje = "warning";

                return RedirectToPage();
            }

            bool correoRepetido = await dbContext.Proveedor
                .AnyAsync(p =>
                    p.ProveedorId != ProveedorId &&
                    p.Correo.ToLower() == correoNormalizado);

            if (correoRepetido)
            {
                TituloMensaje = "Correo repetido";
                Mensaje = "Ya existe otro proveedor registrado con ese correo.";
                TipoMensaje = "warning";

                return RedirectToPage();
            }

            proveedor.Nombre = nombre;
            proveedor.Telefono = telefono;
            proveedor.Correo = correo;
            proveedor.Activo = Activo;

            await dbContext.SaveChangesAsync();

            TituloMensaje = "Proveedor actualizado";
            Mensaje = "Los datos del proveedor se actualizaron correctamente.";
            TipoMensaje = "success";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEliminarAsync()
    {
        var validacion = ValidarAdministrador();

        if (validacion != null)
        {
            return validacion;
        }

        using (dbContext = new punto_de_ventaContext())
        {
            var proveedor = await dbContext.Proveedor
                .FirstOrDefaultAsync(p => p.ProveedorId == ProveedorId);

            if (proveedor == null)
            {
                TituloMensaje = "Proveedor no encontrado";
                Mensaje = "No se pudo encontrar el proveedor que intentas eliminar.";
                TipoMensaje = "danger";

                return RedirectToPage();
            }

            List<string> relaciones = new();

            bool tienePedidos = await dbContext.HistorialPedido
                .AnyAsync(p => p.ProveedorId == ProveedorId);

            bool tieneProductos = await dbContext.Producto
                .AnyAsync(p => p.ProveedorId == ProveedorId);

            if (tienePedidos)
            {
                relaciones.Add("pedidos");
            }

            if (tieneProductos)
            {
                relaciones.Add("productos");
            }

            if (relaciones.Any())
            {
                ProveedorIdDesactivar = proveedor.ProveedorId.ToString();
                NombreProveedorDesactivar = proveedor.Nombre;
                MensajeDesactivar = "El proveedor no se puede eliminar porque tiene "
                    + string.Join(", ", relaciones)
                    + " registrados.";

                return RedirectToPage();
            }

            dbContext.Proveedor.Remove(proveedor);
            await dbContext.SaveChangesAsync();

            TituloMensaje = "Proveedor eliminado";
            Mensaje = "El proveedor se eliminó correctamente.";
            TipoMensaje = "success";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDesactivarAsync()
    {
        var validacion = ValidarAdministrador();

        if (validacion != null)
        {
            return validacion;
        }

        using (dbContext = new punto_de_ventaContext())
        {
            var proveedor = await dbContext.Proveedor
                .FirstOrDefaultAsync(p => p.ProveedorId == ProveedorId);

            if (proveedor == null)
            {
                TituloMensaje = "Proveedor no encontrado";
                Mensaje = "No se pudo encontrar el proveedor que intentas desactivar.";
                TipoMensaje = "danger";

                return RedirectToPage();
            }

            if (proveedor.Activo == false)
            {
                TituloMensaje = "Proveedor ya inactivo";
                Mensaje = "El proveedor ya se encontraba desactivado.";
                TipoMensaje = "info";

                return RedirectToPage();
            }

            proveedor.Activo = false;
            await dbContext.SaveChangesAsync();

            TituloMensaje = "Proveedor desactivado";
            Mensaje = "El proveedor no fue eliminado, pero se desactivó correctamente.";
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

        await CargarProveedoresAsync(ParametroDeBusqueda);

        return Page();
    }

    private async Task CargarProveedoresAsync(string busqueda = "")
    {
        using (dbContext = new punto_de_ventaContext())
        {
            string parametro = busqueda?.Trim() ?? "";

            IQueryable<Proveedor> consulta = dbContext.Proveedor
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(parametro))
            {
                consulta = consulta.Where(p =>
                    p.Nombre.Contains(parametro) ||
                    p.Telefono.Contains(parametro) ||
                    p.Correo.Contains(parametro));
            }

            Proveedores = await consulta
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }
    }

    private bool ValidarDatosProveedor(string nombre, string telefono, string correo)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            TituloMensaje = "Nombre inválido";
            Mensaje = "El nombre del proveedor no puede estar vacío.";
            TipoMensaje = "warning";

            return false;
        }

        if (nombre.Length > 50)
        {
            TituloMensaje = "Nombre demasiado largo";
            Mensaje = "El nombre del proveedor no puede superar los 50 caracteres.";
            TipoMensaje = "warning";

            return false;
        }

        if (string.IsNullOrWhiteSpace(telefono))
        {
            TituloMensaje = "Teléfono inválido";
            Mensaje = "El teléfono del proveedor no puede estar vacío.";
            TipoMensaje = "warning";

            return false;
        }

        if (!Regex.IsMatch(telefono, @"^\d{10}$"))
        {
            TituloMensaje = "Teléfono inválido";
            Mensaje = "El teléfono debe contener exactamente 10 dígitos numéricos.";
            TipoMensaje = "warning";

            return false;
        }

        if (string.IsNullOrWhiteSpace(correo))
        {
            TituloMensaje = "Correo inválido";
            Mensaje = "El correo del proveedor no puede estar vacío.";
            TipoMensaje = "warning";

            return false;
        }

        if (correo.Length > 30)
        {
            TituloMensaje = "Correo demasiado largo";
            Mensaje = "El correo del proveedor no puede superar los 30 caracteres.";
            TipoMensaje = "warning";

            return false;
        }

        if (!Regex.IsMatch(correo, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            TituloMensaje = "Correo inválido";
            Mensaje = "Ingresa un correo válido.";
            TipoMensaje = "warning";

            return false;
        }

        return true;
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