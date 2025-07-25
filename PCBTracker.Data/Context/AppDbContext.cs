using Microsoft.EntityFrameworkCore;
using PCBTracker.Domain.Entities;

namespace PCBTracker.Data.Context;

/// <summary>
/// Represents the EF Core database context for the application.
/// Manages entity mapping and configuration for database interactions.
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of AppDbContext with options provided by the DI container.
    /// These options typically include connection string and provider configuration.
    /// </summary>
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // -------------------------
    // DbSet<T> Definitions
    // -------------------------

    // Represents the Boards table in the database.
    public DbSet<Board> Boards { get; set; } = null!;

    // Represents the Skids table. Each skid groups related boards.
    public DbSet<Skid> Skids { get; set; } = null!;

    // Represents the Users table used for authentication.
    public DbSet<User> Users { get; set; } = null!;

    // Each of the following represents a table for a specific board type:

    public DbSet<LE> LE { get; set; } = null!;
    public DbSet<LE_Upgrade> LE_Upgrade { get; set; } = null!;
    public DbSet<SAD> SAD { get; set; } = null!;
    public DbSet<SAD_Upgrade> SAD_Upgrade { get; set; } = null!;
    public DbSet<SAT> SAT { get; set; } = null!;
    public DbSet<SAT_Upgrade> SAT_Upgrade { get; set; } = null!;

    /// <summary>
    /// Configures entity-level database rules using the Fluent API.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Enforces unique SerialNumber on the Boards table.
        modelBuilder.Entity<Board>()
            .HasIndex(b => b.SerialNumber)
            .IsUnique();

        // Enforces unique SerialNumber on the LE table.
        modelBuilder.Entity<LE>()
            .HasIndex(b => b.SerialNumber)
            .IsUnique();

        // Enforces unique SerialNumber on the LE_Upgrade table.
        modelBuilder.Entity<LE_Upgrade>()
            .HasIndex(b => b.SerialNumber)
            .IsUnique();

        // Enforces unique SerialNumber on the SAD table.
        modelBuilder.Entity<SAD>()
            .HasIndex(b => b.SerialNumber)
            .IsUnique();

        // Enforces unique SerialNumber on the SAD_Upgrade table.
        modelBuilder.Entity<SAD_Upgrade>()
            .HasIndex(b => b.SerialNumber)
            .IsUnique();

        // Enforces unique SerialNumber on the SAT table.
        modelBuilder.Entity<SAT>()
            .HasIndex(b => b.SerialNumber)
            .IsUnique();

        // Enforces unique SerialNumber on the SAT_Upgrade table.
        modelBuilder.Entity<SAT_Upgrade>()
            .HasIndex(b => b.SerialNumber)
            .IsUnique();
    }
}
