using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;
using System.Text.Json;

namespace Proyecto.Pages;

public class EmpleadosModel : PageModel
{
    private punto_de_ventaContext? dbContext;
    public Usuario? UsuarioActual { get; set; }
    [BindProperty]
    public Usuario NuevoUsuario { get; set; } = new();
    public Tienda? TiendaActual { get; set; }
    public List<Tienda> Tiendas { get; set; } = new();
    public List<Usuario> Usuarios { get; set;} = new();
    public List<Venta> Ventas { get; set; } = new();
    /*Datos de nuevo usuario*/
    [BindProperty]
    public int TiendaId { get; set; }
    [BindProperty]
    public int UsuarioId { get; set; }
    [BindProperty]
    public string Nombre { get; set; } = null!;
    [BindProperty]
    public string Telefono { get; set; } = "";
    [BindProperty]
    public string Contrasena { get; set; } = null!;
    [BindProperty]
    public string Rol { get; set; } = null!;
    [BindProperty]
    public bool? Trabajando { get; set; }
    [BindProperty]
    public bool? Contratado { get; set; }
    [BindProperty]
    public string ParametroDeBusqueda { get; set; } = "";

    /*Cargar datos*/
    public async Task OnGetAsync()
    {
        /*Usuario actual*/
        var Json = HttpContext.Session.GetString("Usuario");
        if (Json != null)
        {
            UsuarioActual =
            JsonSerializer.Deserialize<Usuario>(Json);
        }

        /*Tienda*/
        CargarTiendaActual();
        
        using (dbContext = new punto_de_ventaContext())
        {
            /*Tiendas*/
            Tiendas = await dbContext.Tienda.ToListAsync();
            if (TiendaActual != null)
            {
                /*Usuarios de la tienda*/
                Usuarios = await dbContext.Usuario
                    .Where(u => u.TiendaId == TiendaActual.TiendaId)
                    .ToListAsync();
            }
        }
    }
    /*Registrar Usuarios*/
    public async Task<IActionResult> OnPostRegistrarEmpleadoAsync()
    {
        /*Registrar usuario*/
        CargarTiendaActual();
        
        using (dbContext = new punto_de_ventaContext())
        {
            if (TiendaActual == null)
            {
                return RedirectToPage();
            }
            Usuario NuevoUsuario = new Usuario()
            {
                TiendaId = TiendaActual.TiendaId,
                Nombre = Nombre,
                Telefono = Telefono,
                Contrasena = Contrasena,
                Rol = Rol,
                Trabajando = false,
                Contratado = true,
            };
            dbContext.Usuario.Add(NuevoUsuario);
            await dbContext.SaveChangesAsync();
        }
        return RedirectToPage();
    }

    /*Metodo para cargar tiendas*/
    private void CargarTiendaActual()
    {
        var json = HttpContext.Session.GetString("Tienda");

        Console.WriteLine("Contenido de la sesión Tienda: " + json);

        if (json != null)
        {
            TiendaActual = JsonSerializer.Deserialize<Tienda>(json);
        }
    }

    public async Task<IActionResult> OnPostEditarEmpleadoAsync()
    {
        using (dbContext = new punto_de_ventaContext())
        {
            var EditarUsuario = await dbContext.Usuario.Where(u => u.UsuarioId == UsuarioId).FirstOrDefaultAsync();
            if(EditarUsuario != null)
            {
                EditarUsuario.TiendaId = TiendaId;
                EditarUsuario.Nombre = Nombre;
                EditarUsuario.Telefono = Telefono;
                EditarUsuario.Contrasena = Contrasena;
                EditarUsuario.Rol = Rol;
                EditarUsuario.Contratado = Contratado;
                await dbContext.SaveChangesAsync();
            }
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostBusquedaAsync()
    {
        CargarTiendaActual();

        using (dbContext = new punto_de_ventaContext())
        {
            /*Tiendas*/
            Tiendas = await dbContext.Tienda.ToListAsync();
            if (string.IsNullOrWhiteSpace(ParametroDeBusqueda))
            {
                if (TiendaActual != null)
                {
                    /*Usuarios de la tienda*/
                    Usuarios = await dbContext.Usuario
                        .Where(u => u.TiendaId == TiendaActual.TiendaId)
                        .ToListAsync();
                }
            } else
            {
                if (TiendaActual != null)
                {
                    /*Usuarios de la tienda*/
                    Usuarios = await dbContext.Usuario
                        .Where(u => u.TiendaId == TiendaActual.TiendaId &&
                        (u.Nombre.Contains(ParametroDeBusqueda) ||
                        u.Telefono.Contains(ParametroDeBusqueda)))
                        .ToListAsync();
                }
            }
        }
        return Page();
    }
}
