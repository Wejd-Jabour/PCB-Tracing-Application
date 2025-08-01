namespace PCBTracker.UI;

public partial class App : Application
{
    public static PCBTracker.Domain.Entities.User? CurrentUser { get; set; }

    public App()
    {
        InitializeComponent();
        RedirectConsoleOutputToFile();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        if (CurrentUser is null)
        {
            return new Window(new LoginShell());
        }
        else
        {
            return new Window(new AppShell());
        }
    }

    /// <summary>
    /// Redirects Console.WriteLine and Console.Error output to a log file on the Desktop.
    /// Useful for capturing issues in Release builds (.exe) where no console is shown.
    /// </summary>
    private void RedirectConsoleOutputToFile()
    {
        try
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string logFile = Path.Combine(desktopPath, "PCBTracker_log.txt");

            // Open log file for append
            var fileStream = new FileStream(logFile, FileMode.Append, FileAccess.Write);
            var writer = new StreamWriter(fileStream) { AutoFlush = true };

            Console.SetOut(writer);
            Console.SetError(writer);

            Console.WriteLine($"\n=== PCBTracker Log Started {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===");
        }
        catch (Exception ex)
        {
            // Fallback display if log file can't be written
            MainThread.BeginInvokeOnMainThread(() =>
            {
                App.Current.MainPage?.DisplayAlert("Logging Error", ex.Message, "OK");
            });
        }
    }
}
