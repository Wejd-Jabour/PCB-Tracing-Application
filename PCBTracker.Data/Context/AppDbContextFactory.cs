using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PCBTracker.Data.Context
{
    /// <summary>
    /// Provides a factory for creating AppDbContext instances at design time.
    /// Required by EF Core tools such as 'dotnet ef migrations' when the application's runtime
    /// dependency injection setup is not available.
    /// </summary>
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        /// <summary>
        /// Creates and returns a new AppDbContext configured for design-time operations.
        /// This method is automatically called by EF Core CLI tools when executing commands
        /// such as migrations or scaffolding.
        /// </summary>
        public AppDbContext CreateDbContext(string[] args)
        {
            // Manually constructs the DbContextOptions for AppDbContext.
            // This bypasses runtime service configuration and directly specifies the database provider.

            var builder = new DbContextOptionsBuilder<AppDbContext>();

            // Configures the context to use SQL Server with a local development database.
            // Data Source: Uses LocalDB instance.
            // Initial Catalog: Sets the database name to "PCBTracking".
            // Integrated Security: Enables Windows Authentication (no username/password required).
            builder.UseSqlServer(
                @"Data Source=(LocalDB)\MSSQLLocalDB;
                   Initial Catalog=PCBTracking;
                   Integrated Security=True;"
            );

            // Returns a new AppDbContext instance using the constructed options.
            // This object is used by EF Core tooling to inspect the model or apply migrations.
            return new AppDbContext(builder.Options);
        }
    }
}
