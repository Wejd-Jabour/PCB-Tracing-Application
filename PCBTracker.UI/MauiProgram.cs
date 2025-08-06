using BCrypt.Net;                              // Provides BCrypt hashing for creating secure password hashes during seeding
using Microsoft.EntityFrameworkCore;           // EF Core APIs for configuring the DbContext
using Microsoft.Extensions.Logging;            // Logging abstractions for capturing diagnostic output
using PCBTracker.Data.Context;                 // Your AppDbContext, containing DbSet<Board>, DbSet<Skid>, DbSet<User>
using PCBTracker.Domain.Entities;              // Domain entity classes (e.g. User) used during seeding
using PCBTracker.Services;                     // Concrete implementations of your business services
using PCBTracker.Services.Interfaces;          // Interfaces like IUserService and IBoardService
using PCBTracker.UI.ViewModels;                // ViewModel classes for your XAML pages
using PCBTracker.UI.Views;

namespace PCBTracker.UI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        // Create the MAUI app builder, which will collect all configuration
        var builder = MauiApp.CreateBuilder();

        // Tell MAUI which App subclass to instantiate, and register any custom fonts
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                // Register OpenSans font files under friendly aliases
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        // In debug builds, enable detailed debug logging to help during development
        builder.Logging.AddDebug();
#endif

        // === Configure Entity Framework Core ===
        // Define the SQL Server connection string targeting LocalDB for easy local development.
        // - Integrated Security uses Windows Authentication.
        // - Initial Catalog specifies the database name; LocalDB will create it if it doesn't exist.
        var connStr = @"Server=192.168.2.10;
                Database=McLevinShipping;
                User Id=rw_atxshipping;
                Password=Queen7-Friendly-Sale;
                TrustServerCertificate=True;";
        builder.Services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlServer(connStr));


        // === Register application services for Dependency Injection ===
        builder.Services.AddSingleton<IInspectionService, InspectionService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IBoardService, BoardService>();
        builder.Services.AddScoped<IAssemblyCompletionService, AssemblyCompletionService>();


        // === Register ViewModels and Pages ===
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<SubmitViewModel>();
        builder.Services.AddTransient<DataExtractViewModel>();
        builder.Services.AddTransient<DataExtractPage>();
        builder.Services.AddTransient<EditViewModel>();

        builder.Services.AddTransient<InspectionViewModel>();
        builder.Services.AddTransient<InspectionPage>();

        builder.Services.AddTransient<SettingViewModel>();

        builder.Services.AddSingleton(new ConnectionStatusService(connStr));
        builder.Services.AddSingleton<ConnectionStatusViewModel>();

        // Build the configured MAUI application
        var app = builder.Build();

        // === Apply Migrations & Seed Data ===
        // At startup, we ensure the database schema is up to date
        // and that at least one admin user exists for first-time use.
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Applies any pending EF Core migrations (auto-creates/updates tables)
            db.Database.Migrate();

            // Seed a default admin user if the Users table is empty
            if (!db.Users.Any())
            {
                var admin = new User
                {
                    EmployeeID = 1,
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"), 
                    FirstName = "admin",
                    LastName = "admin",
                    Admin = true,
                    Edit = true,
                    Scan = true,
                    Extract = true,
                    Inspection = true
                };
                db.Users.Add(admin);
                db.SaveChanges();
            }
        }

        // Return the fully built app, ready to run
        return app;
    }
}
