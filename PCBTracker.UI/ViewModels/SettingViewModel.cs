using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PCBTracker.Domain.DTOs;
using PCBTracker.Services;
using PCBTracker.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        /// <summary>
        /// Backing property for the entered username.
        /// The [ObservableProperty] attribute generates a public Username property
        /// with automatic INotifyPropertyChanged support.
        /// This is bound two-way in XAML to the username input field.
        /// </summary>
        [ObservableProperty]
        private string newUsername;

        /// <summary>
        /// Backing property for the entered password.
        /// The [ObservableProperty] attribute generates a public Password property.
        /// This is typically bound to a password Entry in XAML with IsPassword=true.
        /// </summary>
        [ObservableProperty]
        private string newPassword;


        //[ObservableProperty]
        //private string _verify;


        [ObservableProperty]
        bool adminPermission = false;

        [ObservableProperty]
        bool scanPermission = false;

        [ObservableProperty]
        bool editPermission = false;

        [ObservableProperty]
        bool inspectionPermission = false;

        [ObservableProperty]
        private bool isUserCreateSectionVisible = false;

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

                if (App.CurrentUser == null || string.IsNullOrEmpty(App.CurrentUser.PasswordHash))
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Current user session is invalid.", "OK");
                    return;
                }

                //if (!BCrypt.Net.BCrypt.Verify(_verify, App.CurrentUser.PasswordHash))
                //{
                //    await App.Current.MainPage.DisplayAlert("Incorrect Password", "The password entered does not match the user's.", "OK");
                //    return;
                //}

                if (!App.CurrentUser.Admin)
                {
                    await App.Current.MainPage.DisplayAlert("Invalid Permission", "You do not have permission to perform this action.", "OK");
                    return;
                }


                _userService.CreateUser(newUsername, newPassword, adminPermission, scanPermission, editPermission, inspectionPermission);

                await App.Current.MainPage.DisplayAlert("Success", "User created successfully.", "OK");

                NewUsername = string.Empty;
                NewPassword = string.Empty;
                //Verify = string.Empty;

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

        private async Task ChangePermissions()
        {

        }

    }
}
