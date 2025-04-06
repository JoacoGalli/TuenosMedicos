using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using TurnosMedicos.Areas.Identity;
using TurnosMedicos.Data;
using Radzen;
using Serilog;
using System.Globalization;

var defaultCulture = new CultureInfo("es-AR"); // o "es-ES"
CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog para escribir logs en un archivo
Log.Logger = new LoggerConfiguration()
    .WriteTo.File("logs/app.log", rollingInterval: RollingInterval.Day) // Guarda logs diarios
    .MinimumLevel.Information() // Solo registra desde nivel Información en adelante
    .CreateLogger();

builder.Host.UseSerilog(); // Integrar Serilog en la app

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();


builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()     
      .AddErrorDescriber<ErroresRegistroEnEspañol>();

builder.Services.Configure<IdentityOptions>(options =>
{
    // Configuración de contraseñas
    options.Password.RequireUppercase = false;       // No requiere mayúsculas
    options.Password.RequireNonAlphanumeric = false; // No requiere caracteres especiales
    options.Password.RequiredLength = 6;             // (Opcional) Mínimo de caracteres
    options.Password.RequireDigit = true;            // (Opcional) Mantiene el requerimiento de números
    
});
builder.Services.AddScoped<RecordatorioService>();
builder.Services.AddScoped<BackupService>();
builder.Services.AddHostedService<HostedService>();


builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();

builder.Services.AddRadzenComponents();

builder.Services.AddSingleton<EmailService>();


var app = builder.Build();

app.UseSerilogRequestLogging(); // Para loguear cada request automáticamente

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
