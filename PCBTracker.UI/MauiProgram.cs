using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PCBTracker.Data.Context;

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
		// Configure EF Core to use LocalDB file
		var connStr =
			@"Data Source=(LocalDB)\MSSQLLocalDB;
			  Initial Catalog=PCBTracking;
			  Integrated Security=True;";

            builder.Services.AddDbContext<AppDbContext>(opts =>
            opts.UseSqlServer(connStr));

        return builder.Build();
    }
}
