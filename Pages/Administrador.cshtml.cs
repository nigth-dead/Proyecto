using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using Proyecto.Models;

namespace Proyecto.Pages;

public class AdministradorModel : PageModel
{
    public Usuario? UsuarioActual { get; set; }
    public void OnGet()
    {
        var json = HttpContext.Session.GetString("Usuario");

        if (json != null)
        {
            UsuarioActual =
                JsonSerializer.Deserialize<Usuario>(json);
        }
    }
}
