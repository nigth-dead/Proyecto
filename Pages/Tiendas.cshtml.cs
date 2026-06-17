using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Proyecto.Models;
using System.Text.Json;

namespace Proyecto.Pages;

public class TiendasModel : PageModel
{
    private punto_de_ventaContext? dbContext { get; set; }
    public Usuario? UsuarioActual { get; set; }
    public List<TiendaVM> Tiendas { get; set; } = new();
    public List<Venta> Ventas { get; set; } = new();
    [BindProperty]
    public String Nombre { get; set; } = "";
    [BindProperty]
    public String Direccion { get; set; } ="";
    [BindProperty]
    public int TiendaId { get; set; }
    [BindProperty]
    public string ParametroDeBusqueda { get; set; } = "";

    
    public async Task OnGetAsync()
    {
        var Json = HttpContext.Session.GetString("Usuario");
        if (Json != null)
        {
            UsuarioActual = JsonSerializer.Deserialize<Usuario>(Json);
        }
        using(dbContext = new punto_de_ventaContext())
        {
            var tiendas = await dbContext.Tienda.ToListAsync();
            await CalcularTotales(tiendas);
        }
    }

    public IActionResult OnGetAbrirEmpleados(int id)
    {
        using(dbContext = new punto_de_ventaContext())
        {
            var tienda = dbContext.Tienda.FirstOrDefault(t => t.TiendaId == id);
            if (tienda != null)
            {
                HttpContext.Session.SetString("Tienda", JsonSerializer.Serialize(tienda));
            }
        }
        return RedirectToPage("/Empleados");
    }

    public IActionResult OnGetAbrirInventario(int id)
    {
        using(dbContext = new punto_de_ventaContext())
        {
            var tienda = dbContext.Tienda.FirstOrDefault(t => t.TiendaId == id);
            if (tienda != null)
            {
                HttpContext.Session.SetString("Tienda", JsonSerializer.Serialize(tienda));
            }
            return RedirectToPage("/DetallesTienda");
        }
    }

    public async Task<IActionResult> OnPostRegistrarAsync()
    {
        using(dbContext = new punto_de_ventaContext())
        {
            Tienda NuevaTienda = new Tienda()
            {
                Nombre = Nombre,
                Direccion = Direccion,
                Estado = true
            };
            dbContext.Add(NuevaTienda);
            dbContext.SaveChanges();
            return RedirectToPage();
        }
    }

    public async Task<IActionResult> OnPostEditarAsync()
    {
        using(dbContext = new punto_de_ventaContext())
        {
            Tienda? tienda = await dbContext.Tienda.Where(i => i.TiendaId == TiendaId).FirstOrDefaultAsync();
            if(tienda != null)
            {
                tienda.Nombre = Nombre;
                tienda.Direccion = Direccion;
                dbContext.SaveChanges();
            }
            return RedirectToPage();
        }
    }

    public async Task<IActionResult> OnPostEliminarAsync(int id)
    {
        using (dbContext = new punto_de_ventaContext())
        {
            var Tienda = await dbContext.Tienda.FindAsync(id);

            if (Tienda == null)
            {
                return RedirectToPage();
            }

            dbContext.Tienda.Remove(Tienda);
            await dbContext.SaveChangesAsync();
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostBusquedaAsync()
    {
        using(dbContext = new punto_de_ventaContext())
        {
            List<Tienda> tiendas;
            if (string.IsNullOrWhiteSpace(ParametroDeBusqueda))
            {
                tiendas = await dbContext.Tienda.ToListAsync();
                
            }
            else
            {
                tiendas = await dbContext.Tienda
                .Where(t => t.Nombre.Contains(ParametroDeBusqueda)||
                t.Direccion.Contains(ParametroDeBusqueda))
                .ToListAsync();
            }
            await CalcularTotales(tiendas);
        }
        return Page();
    }

    private async Task CalcularTotales(List<Tienda> tiendas)
    {
        using(dbContext = new punto_de_ventaContext())
        {
            var totales = await dbContext.Venta
                .Where(v => v.Fecha >= DateTime.Today &&
                v.Fecha < DateTime.Today.AddDays(1))
                .GroupBy(v => v.TiendaId)
                .Select(t => new
                {
                    TiendaId = t.Key,
                    Total = t.Sum(v => v.Total)
                })
                .ToListAsync();

            Tiendas = tiendas.Select(t =>
            {
                var total = totales.FirstOrDefault(x => x.TiendaId == t.TiendaId);

                return new TiendaVM
                {
                    TiendaId = t.TiendaId,
                    Nombre = t.Nombre,
                    Direccion = t.Direccion,
                    Estado = t.Estado,
                    VentaTotal = total?.Total ?? 0
                };
            }).ToList();
        }
    }
}
