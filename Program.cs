using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Dodajemo uslugu za DB context i autentifikaciju
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        // Konfiguriramo identifikaciju i dodajemo podršku za Identity framework s ulogama
        builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.SignIn.RequireConfirmedAccount = false; // Onemogućava potvrdu emaila
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // Dodajemo lažni email sender kako bismo izbjegli grešku
        builder.Services.AddSingleton<IEmailSender, EmailSender>();

        // Konfiguracija autentifikacije pomoću kolačića
        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Identity/Account/Login";   // Ispravljena putanja za prijavu
            options.LogoutPath = "/Identity/Account/Logout"; // Ispravljena putanja za odjavu
            options.AccessDeniedPath = "/Identity/Account/AccessDenied"; // Ispravljena putanja za zabranjen pristup
            options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
            options.SlidingExpiration = true;
        });

        builder.Services.AddRazorPages();
        builder.Services.AddControllersWithViews();

        var app = builder.Build();

        // Provjera okruženja (razvoj, produkcija)
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication(); // Potrebno za autentifikaciju
        app.UseAuthorization();  // Potrebno za autorizaciju

        // Mapiranje rute za MVC i Razor Pages
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
        app.MapRazorPages();

        app.Run();
    }
}