using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using Proyecto.Models;
using Microsoft.EntityFrameworkCore;

namespace Proyecto.Pages;

public class InventarioModel : PageModel
{
    private punto_de_ventaContext? dbContext;
    public string Fecha { get; set; } = "";
    public Usuario? UsuarioActual {get; set;}
    public List<Producto> Productos { get; set; } = new();

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
            Productos = dbContext.Producto.Include(p => p.Inventario).ToList();
        }
    }
}