using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using PCBTracker.UI.ViewModels;

namespace PCBTracker.UI.Views
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();

            // Resolve the LoginViewModel from the DI container
            // (MauiApp was built with these services in MauiProgram.cs)
            var vm =
                Application.Current
                           .Handler
                           .MauiContext
                           .Services
                           .GetRequiredService<LoginViewModel>();

            BindingContext = vm;
        }
    }
}
