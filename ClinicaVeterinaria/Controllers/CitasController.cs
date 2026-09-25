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

        // =========================================================
        // INDEX - LISTAR CITAS
        // =========================================================

        public async Task<IActionResult> Index()
        {
            // ADMINISTRADOR:
            // Puede ver todas las citas.
            if (User.IsInRole("Administrador"))
            {
                var citasAdmin = await _context.Citas
                    .Include(c => c.Mascota)
                    .Include(c => c.ServicioVeterinario)
                    .OrderByDescending(c => c.FechaCita)
                    .ToListAsync();

                return View(citasAdmin);
            }

            // CLIENTE:
            // Solamente puede ver las citas de sus mascotas.
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
                return Challenge();

            var citasCliente = await _context.Citas
                .Include(c => c.Mascota)
                .Include(c => c.ServicioVeterinario)
                .Where(c => c.Mascota != null &&
                            c.Mascota.UsuarioId == usuario.Id)
                .OrderByDescending(c => c.FechaCita)
                .ToListAsync();

            return View(citasCliente);
        }


        // =========================================================
        // CREATE - MOSTRAR FORMULARIO
        // SOLO CLIENTE
        // =========================================================

        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> Create()
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
                return Challenge();

            // Obtener únicamente las mascotas del cliente.
            var mascotas = await _context.Mascotas
                .Where(m => m.UsuarioId == usuario.Id)
                .OrderBy(m => m.Nombre)
                .ToListAsync();

            ViewBag.MascotaId = new SelectList(
                mascotas,
                "Id",
                "Nombre"
            );

            // Obtener servicios disponibles.
            var servicios = await _context.ServiciosVeterinarios
                .OrderBy(s => s.Nombre)
                .ToListAsync();

            ViewBag.ServicioVeterinarioId = new SelectList(
                servicios,
                "Id",
                "Nombre"
            );

            return View();
        }


        // =========================================================
        // CREATE - GUARDAR CITA
        // SOLO CLIENTE
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> Create(Cita cita)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
                return Challenge();

            // -----------------------------------------------------
            // VALIDAR FECHA
            // -----------------------------------------------------

            if (cita.FechaCita < DateTime.Now)
            {
                ModelState.AddModelError(
                    "FechaCita",
                    "La fecha de la cita debe ser futura."
                );
            }

            // -----------------------------------------------------
            // VALIDAR MASCOTA
            // -----------------------------------------------------

            var mascota = await _context.Mascotas
                .FirstOrDefaultAsync(m =>
                    m.Id == cita.MascotaId &&
                    m.UsuarioId == usuario.Id
                );

            if (mascota == null)
            {
                ModelState.AddModelError(
                    "MascotaId",
                    "La mascota seleccionada no pertenece a su cuenta."
                );
            }

            // -----------------------------------------------------
            // VALIDAR SERVICIO
            // -----------------------------------------------------

            var servicio = await _context.ServiciosVeterinarios
                .FirstOrDefaultAsync(s =>
                    s.Id == cita.ServicioVeterinarioId
                );

            if (servicio == null)
            {
                ModelState.AddModelError(
                    "ServicioVeterinarioId",
                    "El servicio seleccionado no existe."
                );
            }

            // -----------------------------------------------------
            // GUARDAR
            // -----------------------------------------------------

            if (ModelState.IsValid)
            {
                // Toda nueva cita comienza como Pendiente.
                cita.Estado = "Pendiente";

                _context.Citas.Add(cita);

                await _context.SaveChangesAsync();

                TempData["Mensaje"] =
                    "La cita fue solicitada correctamente.";

                return RedirectToAction(nameof(Index));
            }

            // -----------------------------------------------------
            // RECARGAR COMBOS SI HAY ERRORES
            // -----------------------------------------------------

            var mascotas = await _context.Mascotas
                .Where(m => m.UsuarioId == usuario.Id)
                .OrderBy(m => m.Nombre)
                .ToListAsync();

            ViewBag.MascotaId = new SelectList(
                mascotas,
                "Id",
                "Nombre",
                cita.MascotaId
            );

            var serviciosDisponibles =
                await _context.ServiciosVeterinarios
                    .OrderBy(s => s.Nombre)
                    .ToListAsync();

            ViewBag.ServicioVeterinarioId = new SelectList(
                serviciosDisponibles,
                "Id",
                "Nombre",
                cita.ServicioVeterinarioId
            );

            return View(cita);
        }


        // =========================================================
        // DETAILS
        // =========================================================

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var cita = await _context.Citas
                .Include(c => c.Mascota)
                .Include(c => c.ServicioVeterinario)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cita == null)
                return NotFound();

            // -----------------------------------------------------
            // CLIENTE:
            // Solamente puede ver sus propias citas.
            // -----------------------------------------------------

            if (User.IsInRole("Cliente"))
            {
                var usuario = await _userManager.GetUserAsync(User);

                if (usuario == null)
                    return Challenge();

                if (cita.Mascota == null ||
                    cita.Mascota.UsuarioId != usuario.Id)
                {
                    return Forbid();
                }
            }

            return View(cita);
        }


        // =========================================================
        // EDIT - MOSTRAR FORMULARIO
        // SOLO ADMINISTRADOR
        // =========================================================

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

            // Lista de mascotas.
            var mascotas = await _context.Mascotas
                .OrderBy(m => m.Nombre)
                .ToListAsync();

            ViewBag.MascotaId = new SelectList(
                mascotas,
                "Id",
                "Nombre",
                cita.MascotaId
            );

            // Lista de servicios.
            var servicios = await _context.ServiciosVeterinarios
                .OrderBy(s => s.Nombre)
                .ToListAsync();

            ViewBag.ServicioVeterinarioId = new SelectList(
                servicios,
                "Id",
                "Nombre",
                cita.ServicioVeterinarioId
            );

            return View(cita);
        }


        // =========================================================
        // EDIT - GUARDAR CAMBIOS
        // SOLO ADMINISTRADOR
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Edit(int id, Cita cita)
        {
            if (id != cita.Id)
                return NotFound();

            // -----------------------------------------------------
            // VALIDAR MASCOTA
            // -----------------------------------------------------

            var mascotaExiste = await _context.Mascotas
                .AnyAsync(m => m.Id == cita.MascotaId);

            if (!mascotaExiste)
            {
                ModelState.AddModelError(
                    "MascotaId",
                    "La mascota seleccionada no existe."
                );
            }

            // -----------------------------------------------------
            // VALIDAR SERVICIO
            // -----------------------------------------------------

            var servicioExiste =
                await _context.ServiciosVeterinarios
                    .AnyAsync(s =>
                        s.Id == cita.ServicioVeterinarioId
                    );

            if (!servicioExiste)
            {
                ModelState.AddModelError(
                    "ServicioVeterinarioId",
                    "El servicio seleccionado no existe."
                );
            }

            // -----------------------------------------------------
            // VALIDAR ESTADO
            // -----------------------------------------------------

            var estadosPermitidos = new[]
            {
                "Pendiente",
                "Atendida",
                "Cancelada"
            };

            if (!estadosPermitidos.Contains(cita.Estado))
            {
                ModelState.AddModelError(
                    "Estado",
                    "El estado seleccionado no es válido."
                );
            }

            // -----------------------------------------------------
            // GUARDAR CAMBIOS
            // -----------------------------------------------------

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cita);

                    await _context.SaveChangesAsync();

                    TempData["Mensaje"] =
                        "La cita fue actualizada correctamente.";

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    var existe = await _context.Citas
                        .AnyAsync(c => c.Id == cita.Id);

                    if (!existe)
                        return NotFound();

                    throw;
                }
            }

            // -----------------------------------------------------
            // RECARGAR COMBOS SI HAY ERRORES
            // -----------------------------------------------------

            var mascotas = await _context.Mascotas
                .OrderBy(m => m.Nombre)
                .ToListAsync();

            ViewBag.MascotaId = new SelectList(
                mascotas,
                "Id",
                "Nombre",
                cita.MascotaId
            );

            var servicios = await _context.ServiciosVeterinarios
                .OrderBy(s => s.Nombre)
                .ToListAsync();

            ViewBag.ServicioVeterinarioId = new SelectList(
                servicios,
                "Id",
                "Nombre",
                cita.ServicioVeterinarioId
            );

            return View(cita);
        }


        // =========================================================
        // DELETE - MOSTRAR CONFIRMACIÓN
        // ADMINISTRADOR Y CLIENTE
        // =========================================================

        [Authorize(Roles = "Administrador,Cliente")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var cita = await _context.Citas
                .Include(c => c.Mascota)
                .Include(c => c.ServicioVeterinario)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cita == null)
                return NotFound();

            // -----------------------------------------------------
            // CLIENTE:
            // Solo puede cancelar citas de sus propias mascotas.
            // -----------------------------------------------------

            if (User.IsInRole("Cliente"))
            {
                var usuario = await _userManager.GetUserAsync(User);

                if (usuario == null)
                    return Challenge();

                if (cita.Mascota == null ||
                    cita.Mascota.UsuarioId != usuario.Id)
                {
                    return Forbid();
                }
            }

            // No permitir cancelar una cita ya cancelada.
            if (cita.Estado == "Cancelada")
            {
                return RedirectToAction(nameof(Index));
            }

            return View(cita);
        }


        // =========================================================
        // DELETE - CANCELAR CITA
        // ADMINISTRADOR Y CLIENTE
        // =========================================================

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Cliente")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cita = await _context.Citas
                .Include(c => c.Mascota)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cita == null)
                return NotFound();

            // -----------------------------------------------------
            // CLIENTE:
            // Solo puede cancelar sus propias citas.
            // -----------------------------------------------------

            if (User.IsInRole("Cliente"))
            {
                var usuario = await _userManager.GetUserAsync(User);

                if (usuario == null)
                    return Challenge();

                if (cita.Mascota == null ||
                    cita.Mascota.UsuarioId != usuario.Id)
                {
                    return Forbid();
                }
            }

            // -----------------------------------------------------
            // CAMBIAR ESTADO
            // -----------------------------------------------------

            cita.Estado = "Cancelada";

            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                "La cita fue cancelada correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}
