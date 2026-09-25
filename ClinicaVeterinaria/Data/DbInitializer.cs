using ClinicaVeterinaria.Data;
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

            string email = "admin@clinica.com";
            string password = "Admin123";

            var admin = await userManager.FindByEmailAsync(email);

            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    NombreCompleto = "Administrador"
                };

                var result = await userManager.CreateAsync(
                    admin,
                    password);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(
                        admin,
                        "Administrador");
                }
            }
        }
    }
}
