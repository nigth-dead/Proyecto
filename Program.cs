using Microsoft.EntityFrameworkCore;
using Proyecto.Models;
using System.Globalization;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();

// SESSION
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();

// DB CONTEXT (MySQL oficial)
builder.Services.AddDbContext<punto_de_ventaContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException("La cadena de conexión 'DefaultConnection' no está configurada.");
    }

    options.UseMySQL(connectionString);
});

var culturaMexico = new CultureInfo("es-MX");

CultureInfo.DefaultThreadCurrentCulture = culturaMexico;
CultureInfo.DefaultThreadCurrentUICulture = culturaMexico;

var app = builder.Build();

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(culturaMexico),
    SupportedCultures = new[] { culturaMexico },
    SupportedUICultures = new[] { culturaMexico }
});

// Configure HTTP pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapRazorPages();

app.Run();