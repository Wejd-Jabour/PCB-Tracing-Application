// SubmitPage.xaml.cs
using System;
using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using PCBTracker.UI.ViewModels;

namespace PCBTracker.UI.Views
{
    /// <summary>
    /// Code-behind for SubmitPage. Resolves the ViewModel via DI,
    /// hooks up entry events, and handles page lifecycle.
    /// </summary>
    public partial class SubmitPage : ContentPage
    {
        public SubmitPage()
        {
            InitializeComponent(); // Load XAML UI

            // Resolve and set the SubmitViewModel from MAUI DI container
            BindingContext = Application.Current
                .Handler
                .MauiContext
                .Services
                .GetRequiredService<SubmitViewModel>();
        }

        /// <summary>
        /// Called when the user finishes entry in the SerialNumber field
        /// (e.g. presses Enter or scans a barcode). Invokes Submit and re-focuses.
        /// </summary>
        private void OnSerialNumberEntryCompleted(object sender, EventArgs e)
        {
            if (BindingContext is SubmitViewModel vm && vm.SubmitCommand.CanExecute(null))
                vm.SubmitCommand.Execute(null);

            SerialNumberEntry.Focus(); // Ready for next scan
        }

        /// <summary>
        /// Handler for a manual Submit button click. Executes SubmitCommand then refocuses.
        /// </summary>
        private void OnSubmitClicked(object sender, EventArgs e)
        {
            if (BindingContext is SubmitViewModel vm && vm.SubmitCommand.CanExecute(null))
                vm.SubmitCommand.Execute(null);

            SerialNumberEntry.Focus();
        }

        /// <summary>
        /// Fires every time the page appears. Loads lookups and sets initial focus.
        /// </summary>
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is SubmitViewModel vm)
                await vm.LoadAsync();

            SerialNumberEntry.Focus();
        }
    }
}
