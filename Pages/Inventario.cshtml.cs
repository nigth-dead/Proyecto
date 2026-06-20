using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using Proyecto.Models;
using Microsoft.EntityFrameworkCore;

namespace Proyecto.Pages;

public class InventarioModel : PageModel
{
    private punto_de_ventaContext? dbContext;

    public string Fecha { get; set; } = "";
    public Usuario? UsuarioActual { get; set; }
    public List<Producto> Productos { get; set; } = new();
    public List<Categoria> Categorias { get; set; } = new();
    public List<Proveedor> Proveedores { get; set; } = new();

    [BindProperty]
    public Producto NuevoProducto { get; set; } = new();

    [BindProperty]
    public Producto ProductoEditar { get; set; } = new();

    [BindProperty]
    public string ParametroDeBusqueda { get; set; } = "";

    [TempData]
    public string? Mensaje { get; set; }

    [TempData]
    public string? TipoMensaje { get; set; }

    [TempData]
    public string? TituloMensaje { get; set; }

    [TempData]
    public string? ProductoIdDesactivar { get; set; }

    [TempData]
    public string? NombreProductoDesactivar { get; set; }

    [TempData]
    public string? MensajeDesactivar { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var validacion = ValidarAdministrador();

        if (validacion != null)
        {
            return validacion;
        }

        Fecha = DateTime.Now.ToString("dd '/' MM '/' yyyy",
            new System.Globalization.CultureInfo("es-MX"));

        await CargarDatosAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostAgregarAsync()
    {
        var validacion = ValidarAdministrador();

        if (validacion != null)
        {
            return validacion;
        }

        NuevoProducto.Nombre = NuevoProducto.Nombre?.Trim() ?? "";
        NuevoProducto.Codigo = NuevoProducto.Codigo?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(NuevoProducto.Nombre))
        {
            TituloMensaje = "Nombre inválido";
            Mensaje = "El nombre del producto no puede estar vacío.";
            TipoMensaje = "warning";

            return RedirectToPage();
        }

        if (string.IsNullOrWhiteSpace(NuevoProducto.Codigo))
        {
            TituloMensaje = "Código inválido";
            Mensaje = "El código del producto no puede estar vacío.";
            TipoMensaje = "warning";

            return RedirectToPage();
        }

        if (!NuevoProducto.Codigo.All(char.IsDigit))
        {
            TituloMensaje = "Código inválido";
            Mensaje = "El código del producto solo debe contener números.";
            TipoMensaje = "warning";

            return RedirectToPage();
        }

        if (NuevoProducto.PrecioCompra < 0)
        {
            TituloMensaje = "Precio inválido";
            Mensaje = "El precio de compra no puede ser negativo.";
            TipoMensaje = "warning";

            return RedirectToPage();
        }

        if (NuevoProducto.PrecioVenta == null || NuevoProducto.PrecioVenta <= 0)
        {
            TituloMensaje = "Precio inválido";
            Mensaje = "El precio de venta debe ser mayor a 0.";
            TipoMensaje = "warning";

            return RedirectToPage();
        }

        using (dbContext = new punto_de_ventaContext())
        {
            bool codigoRepetido = await dbContext.Producto
                .AnyAsync(p => p.Codigo == NuevoProducto.Codigo);

            if (codigoRepetido)
            {
                TituloMensaje = "Código repetido";
                Mensaje = "Ya existe un producto con ese código.";
                TipoMensaje = "warning";

                return RedirectToPage();
            }

            bool categoriaValida = await dbContext.Categoria
                .AnyAsync(c =>
                    c.CategoriaId == NuevoProducto.CategoriaId &&
                    c.Activo == true);

            if (!categoriaValida)
            {
                TituloMensaje = "Categoría inválida";
                Mensaje = "Debes seleccionar una categoría activa.";
                TipoMensaje = "warning";

                return RedirectToPage();
            }

            bool proveedorValido = await dbContext.Proveedor
                .AnyAsync(p =>
                    p.ProveedorId == NuevoProducto.ProveedorId &&
                    p.Activo == true);

            if (!proveedorValido)
            {
                TituloMensaje = "Proveedor inválido";
                Mensaje = "Debes seleccionar un proveedor activo.";
                TipoMensaje = "warning";

                return RedirectToPage();
            }

            NuevoProducto.Activo = true;

            dbContext.Producto.Add(NuevoProducto);
            await dbContext.SaveChangesAsync();

            TituloMensaje = "Producto registrado";
            Mensaje = "El producto se registró correctamente.";
            TipoMensaje = "success";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditarAsync()
    {
        var validacion = ValidarAdministrador();

        if (validacion != null)
        {
            return validacion;
        }

        ProductoEditar.Nombre = ProductoEditar.Nombre?.Trim() ?? "";
        ProductoEditar.Codigo = ProductoEditar.Codigo?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(ProductoEditar.Nombre))
        {
            TituloMensaje = "Nombre inválido";
            Mensaje = "El nombre del producto no puede estar vacío.";
            TipoMensaje = "warning";

            return RedirectToPage();
        }

        if (string.IsNullOrWhiteSpace(ProductoEditar.Codigo))
        {
            TituloMensaje = "Código inválido";
            Mensaje = "El código del producto no puede estar vacío.";
            TipoMensaje = "warning";

            return RedirectToPage();
        }

        if (!ProductoEditar.Codigo.All(char.IsDigit))
        {
            TituloMensaje = "Código inválido";
            Mensaje = "El código del producto solo debe contener números.";
            TipoMensaje = "warning";

            return RedirectToPage();
        }

        if (ProductoEditar.PrecioCompra < 0)
        {
            TituloMensaje = "Precio inválido";
            Mensaje = "El precio de compra no puede ser negativo.";
            TipoMensaje = "warning";

            return RedirectToPage();
        }

        if (ProductoEditar.PrecioVenta == null || ProductoEditar.PrecioVenta <= 0)
        {
            TituloMensaje = "Precio inválido";
            Mensaje = "El precio de venta debe ser mayor a 0.";
            TipoMensaje = "warning";

            return RedirectToPage();
        }

        using (dbContext = new punto_de_ventaContext())
        {
            var producto = await dbContext.Producto
                .FirstOrDefaultAsync(p => p.ProductoId == ProductoEditar.ProductoId);

            if (producto == null)
            {
                TituloMensaje = "Producto no encontrado";
                Mensaje = "No se pudo encontrar el producto que intentas editar.";
                TipoMensaje = "danger";

                return RedirectToPage();
            }

            bool codigoRepetido = await dbContext.Producto
                .AnyAsync(p =>
                    p.ProductoId != ProductoEditar.ProductoId &&
                    p.Codigo == ProductoEditar.Codigo);

            if (codigoRepetido)
            {
                TituloMensaje = "Código repetido";
                Mensaje = "Ya existe otro producto con ese código.";
                TipoMensaje = "warning";

                return RedirectToPage();
            }

            bool categoriaValida = await dbContext.Categoria
                .AnyAsync(c =>
                    c.CategoriaId == ProductoEditar.CategoriaId &&
                    c.Activo == true);

            if (!categoriaValida)
            {
                TituloMensaje = "Categoría inválida";
                Mensaje = "Debes seleccionar una categoría activa.";
                TipoMensaje = "warning";

                return RedirectToPage();
            }

            bool proveedorValido = await dbContext.Proveedor
                .AnyAsync(p =>
                    p.ProveedorId == ProductoEditar.ProveedorId &&
                    p.Activo == true);

            if (!proveedorValido)
            {
                TituloMensaje = "Proveedor inválido";
                Mensaje = "Debes seleccionar un proveedor activo.";
                TipoMensaje = "warning";

                return RedirectToPage();
            }

            producto.Nombre = ProductoEditar.Nombre;
            producto.Codigo = ProductoEditar.Codigo;
            producto.PrecioCompra = ProductoEditar.PrecioCompra;
            producto.PrecioVenta = ProductoEditar.PrecioVenta;
            producto.CategoriaId = ProductoEditar.CategoriaId;
            producto.ProveedorId = ProductoEditar.ProveedorId;
            producto.Activo = ProductoEditar.Activo;

            await dbContext.SaveChangesAsync();

            TituloMensaje = "Producto actualizado";
            Mensaje = "Los datos del producto se actualizaron correctamente.";
            TipoMensaje = "success";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEliminarAsync(int id)
    {
        var validacion = ValidarAdministrador();

        if (validacion != null)
        {
            return validacion;
        }

        using (dbContext = new punto_de_ventaContext())
        {
            var producto = await dbContext.Producto.FindAsync(id);

            if (producto == null)
            {
                TituloMensaje = "Producto no encontrado";
                Mensaje = "No se pudo encontrar el producto que intentas eliminar.";
                TipoMensaje = "danger";

                return RedirectToPage();
            }

            List<string> relaciones = new();

            bool tieneDetalleVenta = await dbContext.DetalleVenta
                .AnyAsync(d => d.ProductoId == id);

            bool tieneInventario = await dbContext.Inventario
                .AnyAsync(i => i.ProductoId == id);

            bool tieneHistorialPedidoDetalle = await dbContext.HistorialPedidoDetalle
                .AnyAsync(h => h.ProductoId == id);

            if (tieneDetalleVenta)
            {
                relaciones.Add("ventas");
            }

            if (tieneInventario)
            {
                relaciones.Add("inventario");
            }

            if (tieneHistorialPedidoDetalle)
            {
                relaciones.Add("pedidos");
            }

            if (relaciones.Any())
            {
                ProductoIdDesactivar = producto.ProductoId.ToString();
                NombreProductoDesactivar = producto.Nombre;
                MensajeDesactivar = "El producto no se puede eliminar porque tiene "
                    + string.Join(", ", relaciones)
                    + " registrados.";

                return RedirectToPage();
            }

            dbContext.Producto.Remove(producto);
            await dbContext.SaveChangesAsync();

            TituloMensaje = "Producto eliminado";
            Mensaje = "El producto se eliminó correctamente.";
            TipoMensaje = "success";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDesactivarAsync(int id)
    {
        var validacion = ValidarAdministrador();

        if (validacion != null)
        {
            return validacion;
        }

        using (dbContext = new punto_de_ventaContext())
        {
            var producto = await dbContext.Producto.FindAsync(id);

            if (producto == null)
            {
                TituloMensaje = "Producto no encontrado";
                Mensaje = "No se pudo encontrar el producto que intentas desactivar.";
                TipoMensaje = "danger";

                return RedirectToPage();
            }

            producto.Activo = false;
            await dbContext.SaveChangesAsync();

            TituloMensaje = "Producto desactivado";
            Mensaje = "El producto no fue eliminado, pero se desactivó correctamente.";
            TipoMensaje = "success";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostBusquedaAsync()
    {
        var validacion = ValidarAdministrador();

        if (validacion != null)
        {
            return validacion;
        }

        Fecha = DateTime.Now.ToString("dd '/' MM '/' yyyy",
            new System.Globalization.CultureInfo("es-MX"));

        await CargarDatosAsync(ParametroDeBusqueda);

        return Page();
    }

    private async Task CargarDatosAsync(string? busqueda = null)
    {
        using (dbContext = new punto_de_ventaContext())
        {
            string parametro = busqueda?.Trim() ?? "";

            Categorias = await dbContext.Categoria
                .AsNoTracking()
                .Where(c => c.Activo == true)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            Proveedores = await dbContext.Proveedor
                .AsNoTracking()
                .Where(p => p.Activo == true)
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            IQueryable<Producto> consulta = dbContext.Producto
                .AsNoTracking()
                .Include(p => p.Inventario)
                .Include(p => p.Categoria)
                .Include(p => p.Proveedor);

            if (!string.IsNullOrWhiteSpace(parametro))
            {
                consulta = consulta.Where(p =>
                    p.Nombre.Contains(parametro) ||
                    p.Codigo.Contains(parametro) ||
                    (p.Categoria != null && p.Categoria.Nombre.Contains(parametro)) ||
                    (p.Proveedor != null && p.Proveedor.Nombre.Contains(parametro)));
            }

            Productos = await consulta
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }
    }

    private IActionResult? ValidarAdministrador()
    {
        var json = HttpContext.Session.GetString("Usuario");

        if (json == null)
        {
            return RedirectToPage("/Index");
        }

        UsuarioActual = JsonSerializer.Deserialize<Usuario>(json);

        if (UsuarioActual == null)
        {
            HttpContext.Session.Remove("Usuario");
            HttpContext.Session.Remove("Tienda");

            return RedirectToPage("/Index");
        }

        if (UsuarioActual.Contratado != true)
        {
            HttpContext.Session.Remove("Usuario");
            HttpContext.Session.Remove("Tienda");

            return RedirectToPage("/Index");
        }

        if (UsuarioActual.Rol != "Administrador")
        {
            return RedirectToPage("/Index");
        }

        return null;
    }
}
