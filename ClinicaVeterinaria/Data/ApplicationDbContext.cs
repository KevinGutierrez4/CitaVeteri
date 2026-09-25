using ClinicaVeterinaria.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ClinicaVeterinaria.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Mascota> Mascotas { get; set; }

        public DbSet<ServicioVeterinario> ServiciosVeterinarios { get; set; }

        public DbSet<Cita> Citas { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Mascota>()
                .HasOne(m => m.Usuario)
                .WithMany(u => u.Mascotas)
                .HasForeignKey(m => m.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Cita>()
                .HasOne(c => c.Mascota)
                .WithMany(m => m.Citas)
                .HasForeignKey(c => c.MascotaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Cita>()
                .HasOne(c => c.ServicioVeterinario)
                .WithMany(s => s.Citas)
                .HasForeignKey(c => c.ServicioVeterinarioId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ServicioVeterinario>()
                .Property(s => s.Precio)
                .HasColumnType("decimal(10,2)");
        }
    }
}
