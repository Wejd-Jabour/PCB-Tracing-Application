using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PCBTracker.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace PCBTracker.UI.ViewModels
{
    public partial class SettingViewModel : ObservableObject
    {
        private readonly IUserService _userService;

        public SettingViewModel(IUserService userService)
        {
            _userService = userService;
        }

        [ObservableProperty]
        private string newEmployeeIDText = string.Empty;

        [ObservableProperty] private string newUsername;
        [ObservableProperty] private string newPassword;
        [ObservableProperty] private string newFirstName;
        [ObservableProperty] private string newLastName;

        [ObservableProperty] bool adminPermission = false;
        [ObservableProperty] bool scanPermission = false;
        [ObservableProperty] bool editPermission = false;
        [ObservableProperty] bool inspectionPermission = false;
        [ObservableProperty] private bool isUserCreateSectionVisible = false;

        public int? ParsedEmployeeID => int.TryParse(NewEmployeeIDText, out var id) ? id : (int?)null;

        [RelayCommand]
        private async Task CreateNewUser()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(newUsername) ||
                    string.IsNullOrWhiteSpace(newPassword))
                {
                    await App.Current.MainPage.DisplayAlert("Missing Fields", "Username, password, and verification must be filled.", "OK");
                    return;
                }

                if (ParsedEmployeeID == null)
                {
                    await App.Current.MainPage.DisplayAlert("Invalid Entry", "Employee ID must be a valid number.", "OK");
                    return;
                }

                if (App.CurrentUser == null || string.IsNullOrEmpty(App.CurrentUser.PasswordHash))
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Current user session is invalid.", "OK");
                    return;
                }

                if (!App.CurrentUser.Admin)
                {
                    await App.Current.MainPage.DisplayAlert("Invalid Permission", "You do not have permission to perform this action.", "OK");
                    return;
                }

                _userService.CreateUser(
                    ParsedEmployeeID.Value,
                    newUsername,
                    newFirstName,
                    newLastName,
                    newPassword,
                    adminPermission,
                    scanPermission,
                    editPermission,
                    inspectionPermission);

                await App.Current.MainPage.DisplayAlert("Success", "User created successfully.", "OK");

                NewEmployeeIDText = string.Empty;
                NewUsername = string.Empty;
                NewPassword = string.Empty;
                NewFirstName = string.Empty;
                NewLastName = string.Empty;

                AdminPermission = false;
                ScanPermission = false;
                EditPermission = false;
                InspectionPermission = false;

                IsUserCreateSectionVisible = false;
            }
            catch (Exception ex)
            {
                string message = !string.IsNullOrWhiteSpace(ex.Message)
                    ? ex.Message
                    : "An unexpected error occurred while saving. Please try again or contact support if it persists.";

                await App.Current.MainPage.DisplayAlert("Error", message, "OK");
            }
        }
    }
}
