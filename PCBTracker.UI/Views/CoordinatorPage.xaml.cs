using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using PCBTracker.UI.ViewModels;

namespace PCBTracker.UI.Views
{
    public partial class CoordinatorPage : ContentPage
    {
        public CoordinatorPage()
        {
            InitializeComponent();

            BindingContext = Application.Current
                .Handler
                .MauiContext
                .Services
                .GetRequiredService<CoordinatorViewModel>();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is CoordinatorViewModel vm)
                await vm.LoadAsync();
        }
    }
}
