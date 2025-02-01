using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using CarRentalApp.Models;

[Authorize]  // Ovdje osiguravamo da samo prijavljeni korisnici mogu pristupiti ovim akcijama
public class CarController : Controller
{
    private readonly ApplicationDbContext _context;

    public CarController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Prikaz svih automobila
    public async Task<IActionResult> Index()
    {
        return View(await _context.Cars.ToListAsync());
    }

    // Prikaz forme za kreiranje novog automobila (samo za admina)
    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        return View();
    }

    // Spremanje novog automobila (samo za admina)
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(Car car)
    {
        if (ModelState.IsValid)
        {
            _context.Add(car);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(car);
    }

    // Prikaz forme za uređivanje automobila (samo za admina)
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var car = await _context.Cars.FindAsync(id);
        if (car == null)
        {
            return NotFound();
        }
        return View(car);
    }

    // Spremanje izmjena za automobil (samo za admina)
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Brand,Model,PricePerDay")] Car car)
    {
        if (id != car.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(car);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CarExists(car.Id))
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
        return View(car);
    }

    // Prikaz forme za brisanje automobila (samo za admina)
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var car = await _context.Cars.FirstOrDefaultAsync(m => m.Id == id);
        if (car == null)
        {
            return NotFound();
        }

        return View(car);
    }

    // Potvrda brisanja automobila (samo za admina)
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var car = await _context.Cars.FindAsync(id);
        if (car != null)
        {
            _context.Cars.Remove(car);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private bool CarExists(int id)
    {
        return _context.Cars.Any(e => e.Id == id);
    }
}
