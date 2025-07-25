namespace PCBTracker.UI;

public partial class App : Application
{
    public static PCBTracker.Domain.Entities.User? CurrentUser { get; set; }
    public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}
}