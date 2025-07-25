using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PCBTracker.Services.Interfaces;
using System.Threading.Tasks;

namespace PCBTracker.UI.ViewModels
{
    /// <summary>
    /// ViewModel for the login page. Exposes bindable username and password fields,
    /// and a command to perform login logic through the IUserService.
    /// Implements INotifyPropertyChanged via ObservableObject.
    /// </summary>
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IUserService _userService;

        /// <summary>
        /// Constructor that receives a reference to the user service for authentication logic.
        /// This service is injected through the dependency injection system.
        /// </summary>
        public LoginViewModel(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Backing property for the entered username.
        /// The [ObservableProperty] attribute generates a public Username property
        /// with automatic INotifyPropertyChanged support.
        /// This is bound two-way in XAML to the username input field.
        /// </summary>
        [ObservableProperty]
        private string _username;

        /// <summary>
        /// Backing property for the entered password.
        /// The [ObservableProperty] attribute generates a public Password property.
        /// This is typically bound to a password Entry in XAML with IsPassword=true.
        /// </summary>
        [ObservableProperty]
        private string _password;

        /// <summary>
        /// Asynchronous login method converted into a command named LoginCommand via [RelayCommand].
        /// When invoked, attempts to authenticate the user and navigate to the next screen.
        /// </summary>
        [RelayCommand]
        private async Task LoginAsync()
        {
            // Call the IUserService.Authenticate method with current input values.
            // This returns a User object if credentials match, or null if invalid.
            var user = _userService.Authenticate(Username, Password);

            // If authentication fails, show an alert dialog to the user.
            if (user is null)
            {
                await App.Current.MainPage.DisplayAlert(
                    "Login Failed",                  // Title
                    "Invalid username or password.", // Message
                    "OK");                           // Button text
            }
            else
            {
                // Store the authenticated user in a static application-level property.
                App.CurrentUser = user;

                // If the current navigation shell is AppShell, load any restricted pages.
                if (Shell.Current is AppShell shell)
                {
                    shell.LoadAuthenticatedPages();
                }

                // Navigate to the root-level SubmitPage using the shell routing system.
                await Shell.Current.GoToAsync("///SubmitPage");
            }
        }
    }
}
