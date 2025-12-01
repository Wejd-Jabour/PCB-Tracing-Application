using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.ApplicationModel;   // MainThread
using Microsoft.Maui.Controls;
using PCBTracker.UI.Views;

namespace PCBTracker.UI
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            RegisterRoutes();

            // Optional: bind a connection status VM if it's registered
            var services = Application.Current?.Handler?.MauiContext?.Services;
            var statusVm = services?.GetService<ConnectionStatusViewModel>();
            if (statusVm != null)
                BindingContext = statusVm;

            BuildTabsForUser(App.CurrentUser);
        }

        // -------- Routes --------
        private static void RegisterRoutes()
        {
            // Register any pages you navigate to via route names
            Routing.RegisterRoute(nameof(SubmitPage), typeof(SubmitPage));
            Routing.RegisterRoute(nameof(DataExtractPage), typeof(DataExtractPage));
            Routing.RegisterRoute(nameof(EditPage), typeof(EditPage));
            Routing.RegisterRoute(nameof(CoordinatorPage), typeof(CoordinatorPage));

            // If you have these pages, uncomment/register them as well:
            // Routing.RegisterRoute(nameof(InspectionPage), typeof(InspectionPage));
            // Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
        }

        // -------- Tabs --------
        private void BuildTabsForUser(PCBTracker.Domain.Entities.User? user)
        {
            // You can tailor tabs by role if needed using `user`
            MainTabBar.Items.Clear();

            AddTab("Submit", "submit", typeof(SubmitPage));
            AddTab("Extract", "extract", typeof(DataExtractPage));
            AddTab("Edit", "edit", typeof(EditPage));
            AddTab("Coordinator", "coordinator", typeof(CoordinatorPage));

            // If you have these pages in your UI project, add them back:
            // AddTab("Inspection", "inspection", typeof(InspectionPage));
            // AddTab("Settings",   "settings",   typeof(SettingsPage));
        }

        private void AddTab(string title, string route, Type pageType)
        {
            var shellContent = new ShellContent
            {
                Title = title,
                Route = route,
                ContentTemplate = new DataTemplate(pageType)
            };

            var tab = new Tab
            {
                Title = title,
                Route = route
            };
            tab.Items.Add(shellContent);

            MainTabBar.Items.Add(tab);
        }

        // -------- Logout --------
        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Logout", "Are you sure you want to log out?", "Logout", "Cancel");
            if (confirm)
                Logout();
        }

        public void Logout()
        {
            // Clear session state
            App.CurrentUser = null;
            App.LoggedInPassword = null; // important for the old-skid unlock flow

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Application.Current.MainPage = new LoginShell();
            });
        }
    }
}
