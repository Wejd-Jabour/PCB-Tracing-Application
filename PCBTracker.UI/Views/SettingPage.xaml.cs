using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PCBTracker.UI.ViewModels;


namespace PCBTracker.UI.Views
{
    public partial class SettingPage : ContentPage
    {
        public SettingPage()
        {
            InitializeComponent();
            BindingContext = Application.Current
                .Handler.MauiContext.Services
                .GetRequiredService<SettingViewModel>();
        }

        private void ToggleUserCreateVisibility(object sender, EventArgs e)
        {
            if (UserCreateSection != null)
                UserCreateSection.IsVisible = !UserCreateSection.IsVisible;
        }

        private void TogglePermissionUpdateVisibility(object sender, EventArgs e)
        {
            if (PermissionUpdateSection != null)
                PermissionUpdateSection.IsVisible = !PermissionUpdateSection.IsVisible;
        }


        private void ToggleUserRemoveVisibility(object sender, EventArgs e)
        {
            if (UserRemoveSection != null)
                UserRemoveSection.IsVisible = !UserRemoveSection.IsVisible;
        }


        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is InspectionViewModel vm)
                await vm.LoadAsync();
        }
    }
}
