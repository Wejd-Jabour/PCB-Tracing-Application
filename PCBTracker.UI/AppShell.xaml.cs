using Microsoft.Maui.Controls;
using PCBTracker.UI.Views;

namespace PCBTracker.UI;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        RegisterRoutes();

        var vm = Application.Current.Handler.MauiContext.Services.GetRequiredService<ConnectionStatusViewModel>();
        BindingContext = vm;

        BuildTabsForUser(App.CurrentUser);
    }

    private void RegisterRoutes()
    {
        Routing.RegisterRoute("SubmitPage", typeof(SubmitPage));
        Routing.RegisterRoute("DataExtract", typeof(DataExtractPage));
        Routing.RegisterRoute("EditPage", typeof(EditPage));
        Routing.RegisterRoute("CoordinatorPage", typeof(CoordinatorPage));
        Routing.RegisterRoute("SettingPage", typeof(SettingPage));
    }

    private void BuildTabsForUser(PCBTracker.Domain.Entities.User? user)
    {
        MainTabBar.Items.Clear();

        if (user == null)
            return;

        if (user.Scan)
            AddTab("Submit", "SubmitPage", typeof(SubmitPage));

        if (user.Extract)
            AddTab("Extract", "DataExtract", typeof(DataExtractPage));

        if (user.Edit)
            AddTab("Edit", "EditPage", typeof(EditPage));

        if (user.Coordinator)
            AddTab("Coordinator", "CoordinatorPage", typeof(CoordinatorPage));

        if (user.Admin)
            AddTab("Settings", "SettingPage", typeof(SettingPage));
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

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Logout", "Are you sure you want to log out?", "Yes", "No");
        if (confirm)
            Logout();
    }

    public void Logout()
    {
        App.CurrentUser = null;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            Application.Current.MainPage = new LoginShell();
        });
    }
}