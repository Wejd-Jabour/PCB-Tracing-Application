using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using PCBTracker.UI.ViewModels;

namespace PCBTracker.UI.Views
{
    public partial class EditPage : ContentPage
    {
        public EditPage()
        {
            InitializeComponent();
            BindingContext = Application.Current
                .Handler
                .MauiContext
                .Services
                .GetRequiredService<EditViewModel>();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is EditViewModel vm)
                await vm.LoadAsync();
        }
    }
}
