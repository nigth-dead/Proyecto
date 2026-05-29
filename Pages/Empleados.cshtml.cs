using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;
using System.Text.Json;

namespace Proyecto.Pages;

public class EmpleadosModel : PageModel
{
    private PuntoDeVentaContext? dbContext;
    public Usuario? UsuarioActual { get; set; }
    [BindProperty]
    public Usuario NuevoUsuario { get; set; } = new();
    public Tienda? TiendaActual { get; set; }
    public List<Tienda> Tiendas { get; set; } = new();
    public List<Usuario> Usuarios { get; set;} = new();
    public List<Venta> Ventas { get; set; } = new();
    /*Datos de nuevo usuario*/
    [BindProperty]
    public int tienda_id { get; set; }
    [BindProperty]
    public string nombre { get; set; } = null!;
    [BindProperty]
    public string? telefono { get; set; }
    [BindProperty]
    public string contrasena { get; set; } = null!;
    [BindProperty]
    public string rol { get; set; } = null!;
    [BindProperty]
    public bool? trabajando { get; set; }
    [BindProperty]
    public bool? contratado { get; set; }

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
        
        using (dbContext = new PuntoDeVentaContext())
        {
            /*Tiendas*/
            Tiendas = await dbContext.Tienda.ToListAsync();
            if (TiendaActual != null)
            {
                /*Usuarios de la tienda*/
                Usuarios = await dbContext.Usuarios
                    .Where(u => u.TiendaId == TiendaActual.TiendaId)
                    .ToListAsync();
            }
        }
    }
    /*Registrar Usuarios*/
    public async Task<IActionResult> OnPostAsync()
    {
        /*Registrar usuario*/
        CargarTiendaActual();
        
        using (dbContext = new PuntoDeVentaContext())
        {
            if (TiendaActual == null)
            {
                return RedirectToPage();
            }
            Usuario NuevoUsuario = new Usuario()
            {
                TiendaId = TiendaActual.TiendaId,
                Nombre = nombre,
                Telefono = telefono,
                Contrasena = contrasena,
                Rol = rol,
                Trabajando = false,
                Contratado = true,
            };
            dbContext.Usuarios.Add(NuevoUsuario);
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
}
