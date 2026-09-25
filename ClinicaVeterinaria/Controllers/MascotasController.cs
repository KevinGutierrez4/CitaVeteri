
using ClinicaVeterinaria.Data;
using ClinicaVeterinaria.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicaVeterinaria.Controllers
{
    [Authorize(Roles = "Cliente")]
    public class MascotasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MascotasController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
                return Challenge();

            var mascotas = await _context.Mascotas
                .Where(m => m.UsuarioId == usuario.Id)
                .OrderBy(m => m.Nombre)
                .ToListAsync();

            return View(mascotas);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Mascota mascota)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
                return Challenge();

            mascota.UsuarioId = usuario.Id;

            ModelState.Remove(nameof(Mascota.UsuarioId));
            ModelState.Remove(nameof(Mascota.Usuario));

            if (!ModelState.IsValid)
                return View(mascota);

            _context.Mascotas.Add(mascota);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Mascota registrada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
                return Challenge();

            var mascota = await _context.Mascotas
                .FirstOrDefaultAsync(m =>
                    m.Id == id &&
                    m.UsuarioId == usuario.Id);

            if (mascota == null)
                return NotFound();

            return View(mascota);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
                return Challenge();

            var mascota = await _context.Mascotas
                .FirstOrDefaultAsync(m =>
                    m.Id == id &&
                    m.UsuarioId == usuario.Id);

            if (mascota == null)
                return NotFound();

            return View(mascota);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Mascota mascota)
        {
            if (id != mascota.Id)
                return NotFound();

            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
                return Challenge();

            var mascotaExistente = await _context.Mascotas
                .FirstOrDefaultAsync(m =>
                    m.Id == id &&
                    m.UsuarioId == usuario.Id);

            if (mascotaExistente == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                mascotaExistente.Nombre = mascota.Nombre;
                mascotaExistente.Especie = mascota.Especie;
                mascotaExistente.Raza = mascota.Raza;

                await _context.SaveChangesAsync();

                TempData["Mensaje"] =
                    "La mascota fue actualizada correctamente.";

                return RedirectToAction(nameof(Index));
            }

            return View(mascota);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
                return Challenge();

            var mascota = await _context.Mascotas
                .FirstOrDefaultAsync(m =>
                    m.Id == id &&
                    m.UsuarioId == usuario.Id);

            if (mascota == null)
                return NotFound();

            return View(mascota);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
                return Challenge();

            var mascota = await _context.Mascotas
                .FirstOrDefaultAsync(m =>
                    m.Id == id &&
                    m.UsuarioId == usuario.Id);

            if (mascota == null)
                return NotFound();

            _context.Mascotas.Remove(mascota);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                "La mascota fue eliminada correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}

