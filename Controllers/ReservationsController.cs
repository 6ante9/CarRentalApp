using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

[Authorize]
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

        if (User.IsInRole("Admin"))
        {
            var allReservations = await _context.Reservations
                .Include(r => r.Car)
                .Include(r => r.User)
                .ToListAsync();
            return View(allReservations);
        }
        else
        {
            var userReservations = await _context.Reservations
                .Include(r => r.Car)
                .Where(r => r.UserId == user.Id)
                .ToListAsync();
            return View(userReservations);
        }
    }

    [Authorize]
    public IActionResult Create(int carId)
    {
        if (carId == 0)
        {
            return NotFound("CarId je nevažeći!");
        }

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
        var user = await _userManager.GetUserAsync(User);

        if (user == null || User.IsInRole("Admin"))
        {
            return Forbid();
        }

        reservation.UserId = user.Id;
        reservation.User = null;
        reservation.Car = await _context.Cars.FindAsync(reservation.CarId);

        if (reservation.Car == null)
        {
            ModelState.AddModelError("CarError", "Odabrani automobil ne postoji.");
            return View(reservation);
        }

        if (reservation.StartDate >= reservation.EndDate)
        {
            ModelState.AddModelError("DateError", "Datum početka mora biti prije datuma završetka.");
        }

        bool isCarAvailable = !_context.Reservations.Any(r => r.CarId == reservation.CarId &&
            ((reservation.StartDate >= r.StartDate && reservation.StartDate <= r.EndDate) ||
             (reservation.EndDate >= r.StartDate && reservation.EndDate <= r.EndDate)));

        if (!isCarAvailable)
        {
            ModelState.AddModelError("CarUnavailable", "Ovaj automobil je već rezerviran u odabranom periodu.");
        }

        ModelState.Remove("User");
        ModelState.Remove("Car");

        if (ModelState.IsValid)
        {
            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        return View(reservation);
    }

    [Authorize]
    public async Task<IActionResult> Cancel(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var reservation = await _context.Reservations.FindAsync(id);

        if (reservation == null)
        {
            return NotFound();
        }

        // Ako korisnik nije admin, može otkazati samo svoju rezervaciju
        if (!User.IsInRole("Admin") && reservation.UserId != user.Id)
        {
            return Forbid();
        }

        _context.Reservations.Remove(reservation);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }
}