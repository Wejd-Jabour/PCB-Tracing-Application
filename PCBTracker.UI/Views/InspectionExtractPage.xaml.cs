using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using PCBTracker.UI.ViewModels;

namespace PCBTracker.UI.Views
{
    public partial class InspectionExtractPage : ContentPage
    {
        public InspectionExtractPage()
        {
            InitializeComponent();
            BindingContext = Application.Current
                .Handler.MauiContext.Services
                .GetRequiredService<InspectionExtractViewModel>();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is InspectionExtractViewModel vm)
                await vm.LoadAsync();
        }
    }
}
