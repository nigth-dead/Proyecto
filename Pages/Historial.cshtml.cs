using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;
using System.Text.Json;

public class HistorialModel : PageModel
{
    public Usuario? UsuarioActual {get; set;}
    private punto_de_ventaContext? dbContext;
    public List<Venta> Ventas { get; set; } = new();

    public async Task OnGetAsync()
    {
        using(dbContext = new punto_de_ventaContext())
        {
            Ventas = await dbContext.Venta.ToListAsync();
        }
        /*Informacion de sesion*/
        var Json = HttpContext.Session.GetString("Usuario");

        if (Json != null)
        {
            UsuarioActual =
            JsonSerializer.Deserialize<Usuario>(Json);
        }
    }
}