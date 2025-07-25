using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using PCBTracker.UI.ViewModels;

namespace PCBTracker.UI.Views
{
    /// <summary>
    /// Code-behind class for the LoginPage XAML view.
    /// Responsible for wiring up event handlers, resolving the ViewModel, and managing UI behavior.
    /// </summary>
    public partial class LoginPage : ContentPage
    {
        // Tracks whether the password is currently hidden or shown in the password entry field.
        private bool _isPasswordHidden = true;

        /// <summary>
        /// Constructor for the LoginPage.
        /// Initializes the UI and sets up ViewModel data binding.
        /// </summary>
        public LoginPage()
        {
            // Loads visual tree and named elements defined in LoginPage.xaml.
            InitializeComponent();

            // Resolves an instance of LoginViewModel from the dependency injection container.
            // This container is set up in MauiProgram.cs and accessed via the current MAUI context.
            var vm = Application
                .Current                   // Gets the global application instance.
                .Handler                   // Accesses the current platform-specific handler.
                .MauiContext               // Provides access to platform services like DI.
                .Services                  // The registered service collection (IServiceProvider).
                .GetRequiredService<LoginViewModel>(); // Throws if the service is not registered.

            // Sets the BindingContext for data bindings defined in XAML.
            // This links ViewModel properties and commands to visual controls.
            BindingContext = vm;

            // Sets initial masking behavior for the password entry field.
            // The PasswordEntry control must be defined in the corresponding XAML file.
            PasswordEntry.IsPassword = _isPasswordHidden;
        }

        /// <summary>
        /// Event handler for the login button or Enter key.
        /// Executes the ViewModel's login command if it can be run.
        /// </summary>
        private void AttemptLogin(object sender, EventArgs e)
        {
            // Checks that BindingContext is the expected ViewModel type,
            // and that the command is currently executable (via CanExecute).
            if (BindingContext is LoginViewModel vm && vm.LoginCommand.CanExecute(null))
                vm.LoginCommand.Execute(null); // Triggers async login logic in the ViewModel.
        }

        /// <summary>
        /// Event handler for toggling password visibility (e.g. clicking an "eye" icon).
        /// Updates the IsPassword state of the Entry field and changes the icon accordingly.
        /// </summary>
        private void OnTogglePasswordVisibility(object sender, EventArgs e)
        {
            // Invert the current visibility flag.
            _isPasswordHidden = !_isPasswordHidden;

            // Update the entry field to match the new visibility setting.
            PasswordEntry.IsPassword = _isPasswordHidden;

            // Get a reference to the button that triggered the event.
            var button = (ImageButton)sender;

            // Set the button image depending on current state.
            // Assumes "hide_password.jpg" and "show_password.jpg" exist in Resources.
            button.Source = _isPasswordHidden ? "hide_password.jpg" : "show_password.jpg";
        }
    }
}
