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
        private void TogglePastInspectionVisibility(object sender, EventArgs e)
        {
            if (PastInspectionSection != null)
                PastInspectionSection.IsVisible = !PastInspectionSection.IsVisible;
        }
        private void ToggleAssemblyVisibility(object sender, EventArgs e)
        {
            if (AssemblyCountSection != null)
                AssemblyCountSection.IsVisible = !AssemblyCountSection.IsVisible;
        }
        private void ToggleAssemblyViewVisibility(object sender, EventArgs e)
        {
            if (AssemblyCountViewSection != null)
                AssemblyCountViewSection.IsVisible = !AssemblyCountViewSection.IsVisible;
        }


        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is InspectionViewModel vm)
                await vm.LoadAsync();
        }

    }
}
