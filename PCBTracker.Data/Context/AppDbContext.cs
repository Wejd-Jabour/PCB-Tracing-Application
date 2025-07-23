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
    public DbSet<LE> LE {  get; set; } = null!;
    public DbSet<LE_Upgrade> LE_Upgrade { get; set; } = null!;
    public DbSet<SAD> SAD { get; set; } = null!; 
    public DbSet<SAD_Upgrade> SAD_Upgrade { get; set; } = null!;
    public DbSet<SAT> SAT { get; set; } = null!;
    public DbSet<SAT_Upgrade> SAT_Upgrade { get; set; } = null!;

    /// <summary>
    /// Configure model rules: indexes, relationships, and optional seed data.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Enforce unique SerialNumber on all tables at the DB level.
        modelBuilder.Entity<Board>()
            .HasIndex(b => b.SerialNumber)
            .IsUnique();
        
        modelBuilder.Entity<LE>()
            .HasIndex(b => b.SerialNumber)
            .IsUnique();
        
        modelBuilder.Entity<LE_Upgrade>()
            .HasIndex(b => b.SerialNumber)
            .IsUnique();
        
        modelBuilder.Entity<SAD>()
            .HasIndex(b => b.SerialNumber)
            .IsUnique();
        
        modelBuilder.Entity<SAD_Upgrade>()
            .HasIndex(b => b.SerialNumber)
            .IsUnique();
        
        modelBuilder.Entity<SAT>()
            .HasIndex(b => b.SerialNumber)
            .IsUnique();
        
        modelBuilder.Entity<SAT_Upgrade>()
            .HasIndex(b => b.SerialNumber)
            .IsUnique();

    }
}