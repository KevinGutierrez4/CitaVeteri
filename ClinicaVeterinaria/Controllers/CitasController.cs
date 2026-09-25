
using ClinicaVeterinaria.Data;
using ClinicaVeterinaria.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ClinicaVeterinaria.Controllers
{
    [Authorize]
    public class CitasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CitasController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // LISTADO DE CITAS
        public async Task<IActionResult> Index()
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
                return Challenge();

            IQueryable<Cita> consulta = _context.Citas
                .Include(c => c.Mascota)
                .Include(c => c.ServicioVeterinario);

            if (User.IsInRole("Cliente"))
            {
                consulta = consulta.Where(c =>
                    c.Mascota.UsuarioId == usuario.Id);
            }

            var citas = await consulta
                .OrderByDescending(c => c.FechaCita)
                .ToListAsync();

            return View(citas);
        }

        // FORMULARIO PARA SOLICITAR CITA
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> Create()
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
                return Challenge();

            var mascotas = await _context.Mascotas
                .Where(m => m.UsuarioId == usuario.Id)
                .OrderBy(m => m.Nombre)
                .ToListAsync();

            var servicios = await _context.ServiciosVeterinarios
                .OrderBy(s => s.Nombre)
                .ToListAsync();

            ViewBag.MascotaId = new SelectList(
                mascotas,
                "Id",
                "Nombre");

            ViewBag.ServicioVeterinarioId = new SelectList(
                servicios,
                "Id",
                "Nombre");

            return View();
        }

        // GUARDAR CITA
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> Create(Cita cita)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
                return Challenge();

            // Validar fecha
            if (cita.FechaCita.Date < DateTime.Today)
            {
                ModelState.AddModelError(
                    "FechaCita",
                    "La fecha de la cita no puede ser anterior a hoy.");
            }

            // Verificar que la mascota pertenece al cliente
            var mascota = await _context.Mascotas
                .FirstOrDefaultAsync(m =>
                    m.Id == cita.MascotaId &&
                    m.UsuarioId == usuario.Id);

            if (mascota == null)
            {
                ModelState.AddModelError(
                    "MascotaId",
                    "La mascota seleccionada no pertenece a este usuario.");
            }

            // Verificar que el servicio existe
            var servicio = await _context.ServiciosVeterinarios
                .FirstOrDefaultAsync(s =>
                    s.Id == cita.ServicioVeterinarioId);

            if (servicio == null)
            {
                ModelState.AddModelError(
                    "ServicioVeterinarioId",
                    "Debe seleccionar un servicio válido.");
            }

            // El estado siempre empieza como Pendiente
            cita.Estado = "Pendiente";

            // Quitamos las validaciones de las propiedades de navegación
            ModelState.Remove(nameof(Cita.Mascota));
            ModelState.Remove(nameof(Cita.ServicioVeterinario));

            if (!ModelState.IsValid)
            {
                var mascotas = await _context.Mascotas
                    .Where(m => m.UsuarioId == usuario.Id)
                    .OrderBy(m => m.Nombre)
                    .ToListAsync();

                var servicios = await _context.ServiciosVeterinarios
                    .OrderBy(s => s.Nombre)
                    .ToListAsync();

                ViewBag.MascotaId = new SelectList(
                    mascotas,
                    "Id",
                    "Nombre",
                    cita.MascotaId);

                ViewBag.ServicioVeterinarioId = new SelectList(
                    servicios,
                    "Id",
                    "Nombre",
                    cita.ServicioVeterinarioId);

                return View(cita);
            }

            _context.Citas.Add(cita);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                "La cita fue solicitada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // DETALLES
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
                return Challenge();

            var cita = await _context.Citas
                .Include(c => c.Mascota)
                .Include(c => c.ServicioVeterinario)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cita == null)
                return NotFound();

            if (User.IsInRole("Cliente") &&
                cita.Mascota.UsuarioId != usuario.Id)
            {
                return Forbid();
            }

            return View(cita);
        }

        // EDITAR ESTADO - ADMINISTRADOR
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var cita = await _context.Citas
                .Include(c => c.Mascota)
                .Include(c => c.ServicioVeterinario)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cita == null)
                return NotFound();

            return View(cita);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Edit(
            int id,
            Cita cita)
        {
            if (id != cita.Id)
                return NotFound();

            if (cita.Estado != "Pendiente" &&
                cita.Estado != "Atendida" &&
                cita.Estado != "Cancelada")
            {
                ModelState.AddModelError(
                    "Estado",
                    "Estado no válido.");
            }

            ModelState.Remove(nameof(Cita.Mascota));
            ModelState.Remove(nameof(Cita.ServicioVeterinario));

            if (ModelState.IsValid)
            {
                var citaExistente = await _context.Citas
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (citaExistente == null)
                    return NotFound();

                citaExistente.FechaCita = cita.FechaCita;
                citaExistente.Estado = cita.Estado;

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(cita);
        }

        // CANCELAR / ELIMINAR
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
                return Challenge();

            var cita = await _context.Citas
                .Include(c => c.Mascota)
                .Include(c => c.ServicioVeterinario)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cita == null)
                return NotFound();

            if (User.IsInRole("Cliente") &&
                cita.Mascota.UsuarioId != usuario.Id)
            {
                return Forbid();
            }

            return View(cita);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
                return Challenge();

            var cita = await _context.Citas
                .Include(c => c.Mascota)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cita == null)
                return NotFound();

            if (User.IsInRole("Cliente"))
            {
                if (cita.Mascota.UsuarioId != usuario.Id)
                    return Forbid();

                cita.Estado = "Cancelada";
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            if (User.IsInRole("Administrador"))
            {
                _context.Citas.Remove(cita);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return Forbid();
        }
    }
}

