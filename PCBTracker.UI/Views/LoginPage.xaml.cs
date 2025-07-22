using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using PCBTracker.UI.ViewModels;

namespace PCBTracker.UI.Views
{
    /// <summary>
    /// Code-behind for the LoginPage XAML view.
    /// Responsible for initializing UI components and wiring up the ViewModel.
    /// </summary>
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent(); // Loads the XAML UI defined in LoginPage.xaml

            // Resolve the LoginViewModel from the MAUI DI container.
            // Application.Current.Handler.MauiContext.Services accesses the IServiceProvider
            // configured in MauiProgram.CreateMauiApp().
            var vm = Application
                .Current
                .Handler
                .MauiContext
                .Services
                .GetRequiredService<LoginViewModel>();

            // Set the BindingContext so that data bindings in XAML connect to the ViewModel properties and commands.
            BindingContext = vm;
        }
    }
}
