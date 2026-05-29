using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;
using System.Text.Json;

namespace Proyecto.Pages;

public class DetallesTiendaModel : PageModel
{
    public Usuario? UsuarioActual { get; set; }
    public async Task OnGetAsync()
    {
        var Json = HttpContext.Session.GetString("Usuario");
        if (Json != null)
        {
            UsuarioActual =
            JsonSerializer.Deserialize<Usuario>(Json);
        }
    }
}
