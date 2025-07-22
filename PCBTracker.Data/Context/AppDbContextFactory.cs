using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PCBTracker.Data.Context
{
    // Implements the design-time factory so EF Core CLI tools can instantiate your DbContext
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        // Called by EF tooling (e.g., 'dotnet ef migrations add') to get a configured DbContext
        public AppDbContext CreateDbContext(string[] args)
        {
            // Build DbContextOptions manually, matching the runtime configuration
            var builder = new DbContextOptionsBuilder<AppDbContext>();
            builder.UseSqlServer(
                @"Data Source=(LocalDB)\MSSQLLocalDB;
                   Initial Catalog=PCBTracking;
                   Integrated Security=True;"
            );

            // Return a new AppDbContext with these options
            return new AppDbContext(builder.Options);
        }
    }
}
