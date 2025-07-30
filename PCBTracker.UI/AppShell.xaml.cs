using Microsoft.Maui.Controls;
using PCBTracker.UI.Views;

namespace PCBTracker.UI;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        RegisterRoutes();

        // Start with only login page visible
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
        Routing.RegisterRoute("InspectionPage", typeof(InspectionPage));
        Routing.RegisterRoute("SettingPage", typeof(SettingPage));
    }

    public void LoadAuthenticatedPages()
    {
        Items.Clear();

        var user = App.CurrentUser;
        if (user == null)
            return;

        // Submit Page (requires Scan permission)
        if (user.Scan)
        {
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
        }

        // Extract Page (requires Extract permission)
        if (user.Extract)
        {
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

        // Edit Page (requires Edit permission)
        if (user.Edit)
        {
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
        }

        // Inspection Page (requires Inspection permission)
        if (user.Inspection)
        {
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
        }

        // Settings Page (requires Admin permission)
        if (user.Admin)
        {
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
        }

        // Logout option (always visible)
        Items.Add(new FlyoutItem
        {
            Title = "Logout",
            Route = "LogoutAction",
            Items =
            {
                new ShellContent
                {
                    ContentTemplate = new DataTemplate(typeof(ContentPage)), // dummy
                    Route = "LogoutAction"
                }
            }
        });
    }

    private void AppShell_Navigating(object sender, ShellNavigatingEventArgs e)
    {
        var target = e.Target?.Location.OriginalString;

        // Handle Logout navigation manually
        if (target?.Contains("LogoutAction") == true)
        {
            e.Cancel();

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                bool confirm = await Shell.Current.DisplayAlert("Logout", "Are you sure you want to log out?", "Yes", "No");
                if (confirm)
                    Logout();
            });

            return;
        }

        // Allow navigating to login
        if (target?.Contains("LoginPage") == true)
            return;

        // Block navigation if user is not logged in
        if (App.CurrentUser == null)
        {
            e.Cancel();
            MainThread.BeginInvokeOnMainThread(() =>
                Shell.Current.GoToAsync("//LoginPage"));
        }
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
}
