using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;
using System.Text.Json;

public class HistorialModel : PageModel
{
    public Usuario? UsuarioActual {get; set;}
    private punto_de_ventaContext? dbContext;
    public List<Venta> Ventas { get; set; } = new();
    [BindProperty]
    public String ParametroDeBusqueda { get; set; } = "";

    public async Task OnGetAsync()
    {
        
        using(dbContext = new punto_de_ventaContext())
        {
            Ventas = await dbContext.Venta
            .Include(v => v.Usuario)
            .ThenInclude(u => u.Tienda)
            .Include(v => v.DetalleVenta)
            .ThenInclude(d => d.Producto)
            .ToListAsync();
        }
        /*Informacion de sesion*/
        var Json = HttpContext.Session.GetString("Usuario");

        if (Json != null)
        {
            UsuarioActual =
            JsonSerializer.Deserialize<Usuario>(Json);
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        using(dbContext = new punto_de_ventaContext())
        {
            if (string.IsNullOrWhiteSpace(ParametroDeBusqueda))
            {
                Ventas = await dbContext.Venta
                .Include(v => v.Usuario)
                .ThenInclude(u => u.Tienda)
                .Include(v => v.DetalleVenta)
                .ThenInclude(d => d.Producto)
                .ToListAsync();
            } else
            {
                if(ParametroDeBusqueda != null)
                {
                    Ventas = await dbContext.Venta
                    .Where(v => v.Usuario.Nombre.Contains(ParametroDeBusqueda.Trim())||
                        v.VentaId.ToString().Contains(ParametroDeBusqueda)||
                        v.Usuario.Tienda.Nombre.Contains(ParametroDeBusqueda))
                    .Include(v => v.Usuario)
                    .ThenInclude(u => u.Tienda)
                    .Include(v => v.DetalleVenta)
                    .ThenInclude(d => d.Producto)
                    .ToListAsync();
                }
            }
        }
        return Page();
    }
}