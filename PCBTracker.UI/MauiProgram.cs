using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PCBTracker.Data.Context;
using PCBTracker.Domain.Entities;    // for the User entity
using BCrypt.Net;                   // for password hashing

namespace PCBTracker.UI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Configure EF Core to use LocalDB
        var connStr =
            @"Data Source=(LocalDB)\MSSQLLocalDB;
              Initial Catalog=PCBTracking;
              Integrated Security=True;";
        builder.Services.AddDbContext<AppDbContext>(opts =>
            opts.UseSqlServer(connStr));

        // Build the app
        var app = builder.Build();

        // --- MIGRATE & SEED ADMIN USER ---
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // 1) Apply any pending migrations
            db.Database.Migrate();

            // 2) If no users exist, create default admin
            if (!db.Users.Any())
            {
                var admin = new User
                {
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
                    Role = "Admin"
                };
                db.Users.Add(admin);
                db.SaveChanges();
            }
        }
        // ----------------------------------

        return app;
    }
}
