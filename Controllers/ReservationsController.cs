using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

public class ReservationsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ReservationsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [Authorize]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        var reservations = await _context.Reservations
            .Include(r => r.Car)
            .Where(r => r.UserId == user.Id)
            .ToListAsync();
        return View(reservations);
    }

    [Authorize]
    public IActionResult Create(int carId)
    {
        if (carId == 0)
        {
            return NotFound("CarId je nevažeći!"); // Bolja greška za debugging
        }

        Console.WriteLine($"Otvorena Create stranica za auto ID: {carId}");

        var reservation = new Reservation
        {
            CarId = carId,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(1)
        };
        return View(reservation);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(Reservation reservation)
    {
        Console.WriteLine("Rezervacija metoda je pokrenuta!");

        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            Console.WriteLine("Greška: Korisnik nije pronađen!");
            return BadRequest("Niste prijavljeni ili korisnik ne postoji.");
        }

        reservation.UserId = user.Id;
        reservation.User = null; // Postavljanje User na NULL da izbjegnemo validaciju
        reservation.Car = await _context.Cars.FindAsync(reservation.CarId);

        if (reservation.Car == null)
        {
            Console.WriteLine($"Greška: Auto sa ID {reservation.CarId} ne postoji!");
            return BadRequest("Odabrani automobil ne postoji.");
        }

        Console.WriteLine($"UserId u aplikaciji: {reservation.UserId}, CarId: {reservation.CarId}");

        ModelState.Remove("User");
        ModelState.Remove("Car");
        ModelState.Remove("UserId");

        if (ModelState.IsValid)
        {
            try
            {
                _context.Reservations.Add(reservation);
                await _context.SaveChangesAsync();
                Console.WriteLine("Rezervacija spremljena u bazu!");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška prilikom spremanja: {ex.Message}");
                return View(reservation);
            }
        }
        else
        {
            Console.WriteLine("ModelState i dalje nije validan! Prikaz grešaka:");
            foreach (var modelStateKey in ModelState.Keys)
            {
                var modelStateVal = ModelState[modelStateKey];
                foreach (var error in modelStateVal.Errors)
                {
                    Console.WriteLine($"Greška u polju {modelStateKey}: {error.ErrorMessage}");
                }
            }
        }

        return View(reservation);
    }

    [Authorize]
    public async Task<IActionResult> Cancel(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var reservation = await _context.Reservations.FirstOrDefaultAsync(r => r.Id == id && r.UserId == user.Id);

        if (reservation == null)
        {
            return NotFound("Rezervacija nije pronađena ili nemate dopuštenje za otkazivanje.");
        }

        _context.Reservations.Remove(reservation);
        await _context.SaveChangesAsync();

        Console.WriteLine($"Rezervacija ID {id} otkazana od strane korisnika {user.Id}");

        return RedirectToAction("Index");
    }
}