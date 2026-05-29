using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;
using System.Text.Json;

public class IndexModel : PageModel
{
    private readonly PuntoDeVentaContext _context;

    [BindProperty]
    public string Nombre { get; set; } = "";

    [BindProperty]
    public string Contrasena { get; set; } = "";

    public string Mensaje { get; set; } = "";

    public IndexModel(PuntoDeVentaContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u =>
                u.Nombre == Nombre &&
                u.Contrasena == Contrasena &&
                u.Contratado == true);

        if (usuario == null)
        {
            Mensaje = "Usuario o contraseña incorrectos.";
            return Page();
        }

        HttpContext.Session.SetString(
            "Usuario",
            JsonSerializer.Serialize(usuario)
        );

        return usuario.Rol switch
        {
            "Administrador" => RedirectToPage("/Administrador"),
            "Cajero" => RedirectToPage("/Cajero"),
            _ => Page()
        };
    }
}