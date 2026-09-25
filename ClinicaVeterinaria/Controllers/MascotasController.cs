using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicaVeterinaria.Models;
using ClinicaVeterinaria.Data;

public class MascotasController : Controller
{
    private readonly ApplicationDbContext _context;

    public MascotasController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: MASCOTAS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Mascotas.ToListAsync());
    }

    // GET: MASCOTAS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var mascota = await _context.Mascotas
            .FirstOrDefaultAsync(m => m.Id == id);
        if (mascota == null)
        {
            return NotFound();
        }

        return View(mascota);
    }

    // GET: MASCOTAS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: MASCOTAS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Nombre,Especie,Raza,UsuarioId,Usuario,Citas")] Mascota mascota)
    {
        if (ModelState.IsValid)
        {
            _context.Add(mascota);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(mascota);
    }

    // GET: MASCOTAS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var mascota = await _context.Mascotas.FindAsync(id);
        if (mascota == null)
        {
            return NotFound();
        }
        return View(mascota);
    }

    // POST: MASCOTAS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,Nombre,Especie,Raza,UsuarioId,Usuario,Citas")] Mascota mascota)
    {
        if (id != mascota.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(mascota);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MascotaExists(mascota.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(mascota);
    }

    // GET: MASCOTAS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var mascota = await _context.Mascotas
            .FirstOrDefaultAsync(m => m.Id == id);
        if (mascota == null)
        {
            return NotFound();
        }

        return View(mascota);
    }

    // POST: MASCOTAS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var mascota = await _context.Mascotas.FindAsync(id);
        if (mascota != null)
        {
            _context.Mascotas.Remove(mascota);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool MascotaExists(int? id)
    {
        return _context.Mascotas.Any(e => e.Id == id);
    }
}
