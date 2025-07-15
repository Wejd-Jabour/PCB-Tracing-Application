using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PCBTracker.Data.Context
{
    public class AppDbContextFactory
        : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<AppDbContext>();
            builder.UseSqlServer(
                @"Data Source=(LocalDB)\MSSQLLocalDB;
                  Initial Catalog=PCBTracking;
                  Integrated Security=True;"
            );

            return new AppDbContext(builder.Options);
        }
    }
}
