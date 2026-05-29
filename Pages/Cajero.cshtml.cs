using Microsoft.AspNetCore.Mvc.RazorPages;
using Proyecto.Models;
using System.Text.Json;

namespace Proyecto.Pages;

public class CajeroModel : PageModel
{
    public Usuario? UsuarioActual {get; set;}
    public string Fecha { get; set; } ="";

    public void OnGet()
    {
        /*Informacion de sesion*/
        var Json = HttpContext.Session.GetString("Usuario");

        if (Json != null)
        {
            UsuarioActual =
            JsonSerializer.Deserialize<Usuario>(Json);
        }

        Fecha = DateTime.Now.ToString("dd '/' MM '/' yyyy", 
            new System.Globalization.CultureInfo("es-MX"));
    }
}
