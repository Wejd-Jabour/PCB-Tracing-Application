using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using PCBTracker.UI.ViewModels;
using System;

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

        private async void OnFilterCompleted(object sender, EventArgs e)
        {
            if (BindingContext is EditViewModel vm)
                await vm.SearchCommand.ExecuteAsync(null);
        }

        private async void OnFilterChanged(object sender, EventArgs e)
        {
            if (BindingContext is EditViewModel vm)
                await vm.SearchCommand.ExecuteAsync(null);
        }
    }
}
