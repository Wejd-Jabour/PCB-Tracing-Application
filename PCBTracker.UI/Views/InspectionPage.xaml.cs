// InspectionPage.xaml.cs
using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using PCBTracker.UI.ViewModels;

namespace PCBTracker.UI.Views
{
    public partial class InspectionPage : ContentPage
    {
        public InspectionPage()
        {
            InitializeComponent();
            BindingContext = Application.Current
                .Handler.MauiContext.Services
                .GetRequiredService<InspectionViewModel>();
        }

        private void ToggleFormVisibility(object sender, EventArgs e)
        {
            if (FormSection != null)
                FormSection.IsVisible = !FormSection.IsVisible;
        }


        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is InspectionViewModel vm)
                await vm.LoadAsync();
        }

    }
}
