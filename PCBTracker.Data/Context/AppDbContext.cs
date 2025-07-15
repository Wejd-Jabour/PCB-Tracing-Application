using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PCBTracker.Domain.Entities;

namespace PCBTracker.Data.Context
{
    // 1) Make class public
    // 2) Inherit from DbContext
    public class AppDbContext : DbContext
    {
        // 3) Keep this constructor
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        { }

        // 4) Public DbSets
        public DbSet<Board> Boards { get; set; } = null!;
        public DbSet<Skid> Skids { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;

        // 5) Override OnModelCreating (call base too)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique index on SerialNumber
            modelBuilder.Entity<Board>()
                .HasIndex(b => b.SerialNumber)
                .IsUnique();
        }
    }
}
