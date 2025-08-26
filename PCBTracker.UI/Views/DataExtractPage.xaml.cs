using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using PCBTracker.UI.ViewModels;

namespace PCBTracker.UI.Views
{
    public partial class DataExtractPage : ContentPage
    {
        public DataExtractPage()
        {
            InitializeComponent();
            BindingContext = Application.Current
             .Handler
             .MauiContext
             .Services
             .GetRequiredService<DataExtractViewModel>();
        }

        private async void OnFilterChanged(object sender, System.EventArgs e)
        {
            if (BindingContext is DataExtractViewModel vm)
                await vm.SearchCommand.ExecuteAsync(null);
        }

        private async void OnFilterCompleted(object sender, System.EventArgs e)
        {
            if (BindingContext is DataExtractViewModel vm)
                await vm.SearchCommand.ExecuteAsync(null);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is DataExtractViewModel vm)
                await vm.LoadAsync();
        }
    }
}
