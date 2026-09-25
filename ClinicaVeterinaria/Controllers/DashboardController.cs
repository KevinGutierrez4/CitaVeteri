
using ClinicaVeterinaria.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicaVeterinaria.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var totalServicios = await _context.ServiciosVeterinarios.CountAsync();

            var totalMascotas = await _context.Mascotas.CountAsync();

            var totalUsuarios = await _context.Users.CountAsync();

            var totalCitas = await _context.Citas.CountAsync();

            var citasPorMes = await _context.Citas
                .GroupBy(c => new
                {
                    c.FechaCita.Year,
                    c.FechaCita.Month
                })
                .Select(g => new
                {
                    Año = g.Key.Year,
                    Mes = g.Key.Month,
                    Total = g.Count()
                })
                .OrderBy(x => x.Año)
                .ThenBy(x => x.Mes)
                .ToListAsync();

            var nombresMeses = new[]
            {
                "Enero",
                "Febrero",
                "Marzo",
                "Abril",
                "Mayo",
                "Junio",
                "Julio",
                "Agosto",
                "Septiembre",
                "Octubre",
                "Noviembre",
                "Diciembre"
            };

            ViewBag.TotalServicios = totalServicios;
            ViewBag.TotalMascotas = totalMascotas;
            ViewBag.TotalUsuarios = totalUsuarios;
            ViewBag.TotalCitas = totalCitas;

            ViewBag.Meses = citasPorMes
                .Select(x => nombresMeses[x.Mes - 1])
                .ToList();

            ViewBag.Cantidades = citasPorMes
                .Select(x => x.Total)
                .ToList();

            return View();
        }
    }
}

