using Microsoft.AspNetCore.Mvc.RazorPages;
using Proyecto.Models;
using System.Text.Json;

public class HistorialModel : PageModel
{
    public Usuario? UsuarioActual {get; set;}

    public void OnGet()
    {
        /*Informacion de sesion*/
        var Json = HttpContext.Session.GetString("Usuario");

        if (Json != null)
        {
            UsuarioActual =
            JsonSerializer.Deserialize<Usuario>(Json);
        }
    }
}