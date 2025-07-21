// PCBTracker.UI/ViewModels/LoginViewModel.cs
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PCBTracker.Services.Interfaces;
using Microsoft.Maui.Controls;

namespace PCBTracker.UI.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IUserService _userService;

        public LoginViewModel(IUserService userService)
        {
            _userService = userService;
        }

        // these generate the Username and Password properties + change notifications
        [ObservableProperty]
        string username;

        [ObservableProperty]
        string password;

        // this generates a LoginCommand ICommand property
        [RelayCommand]
        async Task LoginAsync()
        {
            var user = _userService.Authenticate(Username, Password);
            if (user != null)
            {
                // Absolute Shell route:
                await Shell.Current.GoToAsync("///SubmitPage");
            }
            else
            {
                await Application.Current.MainPage
                    .DisplayAlert("Login Failed", "Invalid credentials", "OK");
            }
        }

    }
}
