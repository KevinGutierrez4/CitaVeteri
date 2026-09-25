using ClinicaVeterinaria.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// CONEXIÓN A LA BASE DE DATOS
// ========================================

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// ========================================
// ASP.NET CORE IDENTITY
// ========================================

builder.Services
    .AddDefaultIdentity<ApplicationUser>(options =>
    {
        // No exigir confirmación de correo
        options.SignIn.RequireConfirmedAccount = false;

        // Configuración de contraseña
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// ========================================
// MVC
// ========================================

builder.Services.AddControllersWithViews();

// Necesario para Identity Razor Pages
builder.Services.AddRazorPages();

var app = builder.Build();

// ========================================
// CREAR ROLES AUTOMÁTICAMENTE
// ========================================

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    await DbInitializer.SeedRolesAsync(services);
}

// ========================================
// CONFIGURACIÓN DEL PIPELINE
// ========================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

// ========================================
// AUTENTICACIÓN Y AUTORIZACIÓN
// ========================================

app.UseAuthentication();

app.UseAuthorization();

// ========================================
// RUTAS MVC
// ========================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ========================================
// RUTAS DE IDENTITY
// ========================================

app.MapRazorPages();

app.Run();
