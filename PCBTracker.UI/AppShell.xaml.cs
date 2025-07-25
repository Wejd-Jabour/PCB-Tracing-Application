using Microsoft.Maui.Controls;
using PCBTracker.UI.Views;
namespace PCBTracker.UI;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        RegisterRoutes();

        // Only show login page by default
        Items.Clear();
        Items.Add(new ShellContent
        {
            Route = "LoginPage",
            ContentTemplate = new DataTemplate(typeof(LoginPage)),
            Title = "Login",
            FlyoutItemIsVisible = false
        });
        Navigating += AppShell_Navigating;
    }
    private void RegisterRoutes()
    {
        Routing.RegisterRoute("SubmitPage", typeof(SubmitPage));
        Routing.RegisterRoute("DataExtract", typeof(DataExtractPage)); 
    }
    public void LoadAuthenticatedPages()
    {
        Items.Clear();

        Items.Add(new FlyoutItem
        {
            Title = "Submit",
            Route = "SubmitPage",
            Items =
        {
            new ShellContent
            {
                ContentTemplate = new DataTemplate(typeof(SubmitPage)),
                Route = "SubmitPage"
            }
        }
        });

        Items.Add(new FlyoutItem
        {
            Title = "Extract",
            Route = "DataExtract",
            Items =
        {
            new ShellContent
            {
                ContentTemplate = new DataTemplate(typeof(DataExtractPage)),
                Route = "DataExtract"
            }
        }
        });
    }
    private void AppShell_Navigating(object sender, ShellNavigatingEventArgs e)
    {
        var target = e.Target?.Location.OriginalString;

        if (target?.Contains("LoginPage") == true)
            return;

        if (App.CurrentUser == null)
        {
            e.Cancel();
            MainThread.BeginInvokeOnMainThread(() =>
                Shell.Current.GoToAsync("//LoginPage"));
        }
    }
}
