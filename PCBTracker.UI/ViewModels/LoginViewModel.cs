using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PCBTracker.Services.Interfaces;
using System.Threading.Tasks;

namespace PCBTracker.UI.ViewModels
{
    /// <summary>
    /// ViewModel for LoginPage. Exposes Username/Password properties,
    /// and a LoginCommand that handles authentication and navigation.
    /// </summary>
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IUserService _userService;

        /// <summary>
        /// IUserService is injected via DI to perform the actual auth logic.
        /// </summary>
        public LoginViewModel(IUserService userService)
        {
            _userService = userService;
        }

        // The entered username, bound two-way to an Entry.Text in XAML.
        [ObservableProperty]
        private string _username;

        // The entered password, bound to an Entry with IsPassword=true.
        [ObservableProperty]
        private string _password;

        /// <summary>
        /// This method is converted by [RelayCommand] into an ICommand named LoginCommand.
        /// It runs when the user taps the “Login” button or presses Enter.
        /// </summary>
        [RelayCommand]
        private async Task LoginAsync()
        {
            // Attempt to authenticate; returns User or null.
            var user = _userService.Authenticate(Username, Password);

            if (user is null)
            {
                // Show an alert if credentials are invalid.
                await App.Current.MainPage.DisplayAlert(
                    "Login Failed",
                    "Invalid username or password.",
                    "OK");
            }
            else
            {
                // On success, navigate to the SubmitPage (shell route).
                await Shell.Current.GoToAsync("///SubmitPage");
            }
        }
    }
}
