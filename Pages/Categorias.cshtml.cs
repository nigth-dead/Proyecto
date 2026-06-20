using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using Proyecto.Models;
using Microsoft.EntityFrameworkCore;

namespace Proyecto.Pages;

public class CategoriasModel : PageModel
{
    private punto_de_ventaContext? dbContext;

    public Usuario? UsuarioActual { get; set; }
    public List<Categoria> Categorias { get; set; } = new();

    [BindProperty]
    public int CategoriaId { get; set; }

    [BindProperty]
    public string Nombre { get; set; } = "";

    [BindProperty]
    public string Descripcion { get; set; } = "";

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
    public string? CategoriaIdDesactivar { get; set; }

    [TempData]
    public string? NombreCategoriaDesactivar { get; set; }

    [TempData]
    public string? MensajeDesactivar { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var validacion = ValidarAdministrador();

        if (validacion != null)
        {
            return validacion;
        }

        await CargarCategoriasAsync();

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
        string descripcion = Descripcion.Trim();

        if (string.IsNullOrWhiteSpace(nombre))
        {
            TituloMensaje = "Nombre inválido";
            Mensaje = "El nombre de la categoría no puede estar vacío.";
            TipoMensaje = "warning";

            return RedirectToPage();
        }

        if (string.IsNullOrWhiteSpace(descripcion))
        {
            TituloMensaje = "Descripción inválida";
            Mensaje = "La descripción de la categoría no puede estar vacía.";
            TipoMensaje = "warning";

            return RedirectToPage();
        }

        using (dbContext = new punto_de_ventaContext())
        {
            bool nombreRepetido = await dbContext.Categoria
                .AnyAsync(c => c.Nombre == nombre);

            if (nombreRepetido)
            {
                TituloMensaje = "Categoría repetida";
                Mensaje = "Ya existe una categoría con ese nombre.";
                TipoMensaje = "warning";

                return RedirectToPage();
            }

            Categoria nuevaCategoria = new Categoria
            {
                Nombre = nombre,
                Descripcion = descripcion,
                Activo = true
            };

            dbContext.Categoria.Add(nuevaCategoria);
            await dbContext.SaveChangesAsync();

            TituloMensaje = "Categoría registrada";
            Mensaje = "La categoría se registró correctamente.";
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
        string descripcion = Descripcion.Trim();

        if (string.IsNullOrWhiteSpace(nombre))
        {
            TituloMensaje = "Nombre inválido";
            Mensaje = "El nombre de la categoría no puede estar vacío.";
            TipoMensaje = "warning";

            return RedirectToPage();
        }

        if (string.IsNullOrWhiteSpace(descripcion))
        {
            TituloMensaje = "Descripción inválida";
            Mensaje = "La descripción de la categoría no puede estar vacía.";
            TipoMensaje = "warning";

            return RedirectToPage();
        }

        using (dbContext = new punto_de_ventaContext())
        {
            var categoria = await dbContext.Categoria
                .FirstOrDefaultAsync(c => c.CategoriaId == CategoriaId);

            if (categoria == null)
            {
                TituloMensaje = "Categoría no encontrada";
                Mensaje = "No se pudo encontrar la categoría que intentas editar.";
                TipoMensaje = "danger";

                return RedirectToPage();
            }

            bool nombreRepetido = await dbContext.Categoria
                .AnyAsync(c =>
                    c.CategoriaId != CategoriaId &&
                    c.Nombre == nombre);

            if (nombreRepetido)
            {
                TituloMensaje = "Categoría repetida";
                Mensaje = "Ya existe otra categoría con ese nombre.";
                TipoMensaje = "warning";

                return RedirectToPage();
            }

            categoria.Nombre = nombre;
            categoria.Descripcion = descripcion;
            categoria.Activo = Activo;

            await dbContext.SaveChangesAsync();

            TituloMensaje = "Categoría actualizada";
            Mensaje = "Los datos de la categoría se actualizaron correctamente.";
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
            var categoria = await dbContext.Categoria
                .FirstOrDefaultAsync(c => c.CategoriaId == CategoriaId);

            if (categoria == null)
            {
                TituloMensaje = "Categoría no encontrada";
                Mensaje = "No se pudo encontrar la categoría que intentas eliminar.";
                TipoMensaje = "danger";

                return RedirectToPage();
            }

            bool tieneProductos = await dbContext.Producto
                .AnyAsync(p => p.CategoriaId == CategoriaId);

            if (tieneProductos)
            {
                CategoriaIdDesactivar = categoria.CategoriaId.ToString();
                NombreCategoriaDesactivar = categoria.Nombre;
                MensajeDesactivar = "La categoría no se puede eliminar porque tiene productos registrados.";

                return RedirectToPage();
            }

            dbContext.Categoria.Remove(categoria);
            await dbContext.SaveChangesAsync();

            TituloMensaje = "Categoría eliminada";
            Mensaje = "La categoría se eliminó correctamente.";
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
            var categoria = await dbContext.Categoria
                .FirstOrDefaultAsync(c => c.CategoriaId == CategoriaId);

            if (categoria != null)
            {
                categoria.Activo = false;

                await dbContext.SaveChangesAsync();

                TituloMensaje = "Categoría desactivada";
                Mensaje = "La categoría no fue eliminada, pero se desactivó correctamente.";
                TipoMensaje = "success";
            }
            else
            {
                TituloMensaje = "Categoría no encontrada";
                Mensaje = "No se pudo encontrar la categoría que intentas desactivar.";
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

        await CargarCategoriasAsync(ParametroDeBusqueda);

        return Page();
    }

    private async Task CargarCategoriasAsync(string? busqueda = null)
    {
        using (dbContext = new punto_de_ventaContext())
        {
            string parametro = busqueda?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(parametro))
            {
                Categorias = await dbContext.Categoria
                    .OrderBy(c => c.Nombre)
                    .ToListAsync();
            }
            else
            {
                Categorias = await dbContext.Categoria
                    .Where(c =>
                        c.Nombre.Contains(parametro) ||
                        (c.Descripcion != null && c.Descripcion.Contains(parametro)))
                    .OrderBy(c => c.Nombre)
                    .ToListAsync();
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

        if (UsuarioActual.Rol != "Administrador")
        {
            return RedirectToPage("/Index");
        }

        return null;
    }
}