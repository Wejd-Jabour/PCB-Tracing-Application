using System;
using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using PCBTracker.UI.ViewModels;

namespace PCBTracker.UI.Views
{
    public partial class SubmitPage : ContentPage
    {
        public SubmitPage()
        {
            InitializeComponent();

            // resolve your VM via DI
            BindingContext = Application
                .Current
                .Handler
                .MauiContext
                .Services
                .GetRequiredService<SubmitViewModel>();
        }

        /// <summary>
        /// Fires when the user (or barcode scanner) presses Enter in the SN Entry.
        /// We call SubmitAsync directly—skipping the Command’s CanExecute—so auto-submits work.
        /// </summary>
        private async void OnSerialNumberEntryCompleted(object sender, EventArgs e)
        {
            if (BindingContext is SubmitViewModel vm)
            {
                // 1) Directly invoke the VM’s async submit method
                await vm.SubmitAsync();

                // 2) Clear SerialNumber (your VM already does this) and re-focus
                SerialNumberEntry.Focus();
            }
        }

        /// <summary>
        /// Fires when the Submit button is clicked manually.
        /// We still focus the Serial box after a manual click.
        /// </summary>
        private void OnSubmitClicked(object sender, EventArgs e)
        {
            Dispatcher.Dispatch(() =>
            {
                SerialNumberEntry.Focus();
            });
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is SubmitViewModel vm)
                await vm.LoadAsync();

            // initial focus for the first scan
            SerialNumberEntry.Focus();
        }
    }
}
