using Microsoft.EntityFrameworkCore;
using PCBTracker.Domain.Entities;

namespace PCBTracker.Data.Context;

/// <summary>
/// EF Core DbContext defining your database schema via DbSet properties
/// and model configuration (indexes, relationships, seed data).
/// </summary>
public class AppDbContext : DbContext
{
    // Constructor: DbContextOptions (connection string, provider, etc.)
    // are injected by the DI container at runtime.
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // Each DbSet<T> corresponds to a table in the database:
    public DbSet<Board> Boards { get; set; } = null!;  // Boards table
    public DbSet<Skid> Skids { get; set; } = null!;    // Skids table
    public DbSet<User> Users { get; set; } = null!;    // Users table for authentication

    /// <summary>
    /// Configure model rules: indexes, relationships, and optional seed data.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Enforce unique SerialNumber on the Boards table at the DB level.
        modelBuilder.Entity<Board>()
            .HasIndex(b => b.SerialNumber)
            .IsUnique();

    }
}