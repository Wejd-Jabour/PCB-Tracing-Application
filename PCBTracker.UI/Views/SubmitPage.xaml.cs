using System;
using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using PCBTracker.UI.ViewModels;

namespace PCBTracker.UI.Views
{
    public partial class SubmitPage : ContentPage
    {
        private SubmitViewModel Vm => BindingContext as SubmitViewModel;
        public SubmitPage()
        {
            InitializeComponent();

            // Resolve the VM from DI
            BindingContext = Application
                .Current
                .Handler
                .MauiContext
                .Services
                .GetRequiredService<SubmitViewModel>();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is SubmitViewModel vm)
                await vm.LoadAsync();
        }
    }
}
