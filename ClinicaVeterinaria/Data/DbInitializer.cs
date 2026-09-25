
using Microsoft.AspNetCore.Identity;

namespace ClinicaVeterinaria.Data
{
    public static class DbInitializer
    {
        public static async Task SeedRolesAsync(
            IServiceProvider serviceProvider)
        {
            var roleManager =
                serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var userManager =
                serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // ==========================================
            // CREAR ROLES
            // ==========================================

            string[] roles =
            {
                "Administrador",
                "Cliente"
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(
                        new IdentityRole(role));
                }
            }

            // ==========================================
            // CREAR ADMINISTRADOR
            // ==========================================

            string adminEmail = "admin@clinica.com";
            string adminPassword = "Admin123";

            var admin = await userManager.FindByEmailAsync(
                adminEmail);

            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    NombreCompleto = "Administrador"
                };

                var result = await userManager.CreateAsync(
                    admin,
                    adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(
                        admin,
                        "Administrador");
                }
            }
            else
            {
                // Si el administrador ya existe,
                // nos aseguramos de que tenga su rol.
                if (!await userManager.IsInRoleAsync(
                    admin,
                    "Administrador"))
                {
                    await userManager.AddToRoleAsync(
                        admin,
                        "Administrador");
                }
            }
        }
    }
}

