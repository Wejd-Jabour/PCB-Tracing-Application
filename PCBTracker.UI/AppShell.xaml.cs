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
        Routing.RegisterRoute("EditPage", typeof(EditPage));

    }
    public void Logout()
    {
        App.CurrentUser = null;

        Items.Clear();
        Items.Add(new ShellContent
        {
            Route = "LoginPage",
            ContentTemplate = new DataTemplate(typeof(LoginPage)),
            Title = "Login",
            FlyoutItemIsVisible = false
        });

        MainThread.BeginInvokeOnMainThread(() =>
            Shell.Current.GoToAsync("//LoginPage"));
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
        Items.Add(new FlyoutItem
        {
            Title = "Edit",
            Route = "EditPage",
            Items =
        {
            new ShellContent
            {
                ContentTemplate = new DataTemplate(typeof(EditPage)),
                Route = "EditPage"
            }
        }
        });

        Items.Add(new FlyoutItem
        {
            Title = "Inspection",
            Route = "InspectionPage",
            Items =
                {
                    new ShellContent
                    {
                        ContentTemplate = new DataTemplate(typeof(InspectionPage)),
                        Route = "InspectionPage"
                    }
                }
        });

        Items.Add(new FlyoutItem
        {
            Title = "Settings",
            Route = "SettingPage",
            Items =
                {
                    new ShellContent
                    {
                        ContentTemplate = new DataTemplate(typeof(SettingPage)),
                        Route = "SettingPage"
                    }
                }
        });
        Items.Add(new FlyoutItem
        {
            Title = "Logout",
            Route = "LogoutAction",
            Items =
    {
        new ShellContent
        {
            ContentTemplate = new DataTemplate(typeof(ContentPage)), // Placeholder page
            Route = "LogoutAction"
        }
    }
        });

    }
    private void AppShell_Navigating(object sender, ShellNavigatingEventArgs e)
    {
        var target = e.Target?.Location.OriginalString;

        if (target?.Contains("LogoutAction") == true)
        {
            e.Cancel(); // Prevent navigation

            bool confirm = true;
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                confirm = await Shell.Current.DisplayAlert("Logout", "Are you sure you want to log out?", "Yes", "No");
                if (confirm)
                    Logout();
            });

            return;
        }

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
