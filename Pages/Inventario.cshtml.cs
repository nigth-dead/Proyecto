using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using Proyecto.Models;
using Microsoft.EntityFrameworkCore;

namespace Proyecto.Pages;

public class InventarioModel : PageModel
{
    private punto_de_ventaContext? dbContext;
    public string Fecha { get; set; } = "";
    public Usuario? UsuarioActual { get; set; }
    public List<Producto> Productos { get; set; } = new();
    public List<Categoria> Categorias { get; set; } = new();

    [BindProperty]
    public Producto NuevoProducto { get; set; } = new();

    [BindProperty]
    public Producto ProductoEditar { get; set; } = new();
    [BindProperty]
    public string ParametroDeBusqueda { get; set; } = "";

    public IActionResult OnPostAgregar()
    {
        using (dbContext = new punto_de_ventaContext())
        {
            NuevoProducto.Activo = true;

            dbContext.Producto.Add(NuevoProducto);
            dbContext.SaveChanges();
        }

        return RedirectToPage();
    }

    public IActionResult OnPostEditar()
    {
        using (dbContext = new punto_de_ventaContext())
        {
            var producto = dbContext.Producto
                .FirstOrDefault(p => p.ProductoId == ProductoEditar.ProductoId);

            if (producto != null)
            {
                producto.Nombre = ProductoEditar.Nombre;
                producto.Codigo = ProductoEditar.Codigo;
                producto.Precio = ProductoEditar.Precio;
                producto.CategoriaId = ProductoEditar.CategoriaId;
                producto.Activo = ProductoEditar.Activo;

                dbContext.SaveChanges();
            }
            
        }

        return RedirectToPage();
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

        /*Fecha*/
        Fecha = DateTime.Now.ToString("dd '/' MM '/' yyyy",
            new System.Globalization.CultureInfo("es-MX"));

        /*Cargar productos*/
        using (dbContext = new punto_de_ventaContext())
        {
            Productos = dbContext.Producto
                .Include(p => p.Inventario)
                .Include(p => p.Categoria)
                .ToList();

            Categorias = dbContext.Categoria
            .ToList();
        }
    }

    public async Task<IActionResult> OnPostEliminarAsync(int id)
    {
        using (dbContext = new punto_de_ventaContext())
        {
            var producto = await dbContext.Producto.FindAsync(id);

            if (producto == null)
            {
                return RedirectToPage();
            }

            dbContext.Producto.Remove(producto);
            await dbContext.SaveChangesAsync();
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostBusquedaAsync()
    {
        using (dbContext = new punto_de_ventaContext())
        {
            if (string.IsNullOrWhiteSpace(ParametroDeBusqueda))
            {
                Productos = dbContext.Producto
                    .Include(p => p.Inventario)
                    .Include(p => p.Categoria)
                    .ToList();
            }
            else
            {
                Productos = dbContext.Producto
                    .Where(p => p.Nombre.Contains(ParametroDeBusqueda)||
                    p.Codigo.Contains(ParametroDeBusqueda)||
                    p.Categoria.Nombre.Contains(ParametroDeBusqueda))
                    .Include(p => p.Inventario)
                    .Include(p => p.Categoria)
                    .ToList();
            }
        }
        return Page();
    }
}

