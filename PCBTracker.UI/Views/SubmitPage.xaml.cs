// SubmitPage.xaml.cs
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using PCBTracker.UI.ViewModels;

namespace PCBTracker.UI.Views
{
    /// <summary>
    /// Code-behind for the SubmitPage view.
    /// Responsible for initializing the page, resolving the ViewModel,
    /// wiring up event handlers, and handling lifecycle events.
    /// </summary>
    public partial class SubmitPage : ContentPage
    {
        /// <summary>
        /// Static dictionary mapping board type strings to their part numbers.
        /// Used to validate or display expected mappings between type and SKU in ViewModel.
        /// Not directly used in this code-behind file, but possibly referenced in XAML binding context.
        /// </summary>
        static readonly IReadOnlyDictionary<string, string> _partNumberMap = new Dictionary<string, string>
        {
            ["LE"] = "ASY-G8GMLESBH-P-ATLR07MR1",
            ["LE Upgrade"] = "Unknown",
            ["SAD"] = "ASY-G8GMSADSBH-P-ATLR03MR1",
            ["SAD Upgrade"] = "ASY-GSGMSADB-UG-KIT-P-ATLR05MR2",
            ["SAT"] = "ASY-G8GMSATSBH-P-ATLR02MR1",
            ["SAT Upgrade"] = "ASY-G8GMSATB-UG-KIT-P-ATLR03MR1",
        };

        public SubmitPage()
        {
            // Builds visual tree from SubmitPage.xaml and wires up named controls.
            InitializeComponent();

            // Resolve an instance of SubmitViewModel using MAUI's DI container.
            BindingContext = Application.Current
                .Handler
                .MauiContext
                .Services
                .GetRequiredService<SubmitViewModel>();
        }

        /// <summary>
        /// Event handler for the 'Completed' event of the SerialNumber entry control.
        /// This is typically triggered by pressing Enter or scanning a barcode.
        /// Only auto-submits when the Auto Submit toggle is ON.
        /// </summary>
        private void OnSerialNumberEntryCompleted(object sender, EventArgs e)
        {
            if (BindingContext is SubmitViewModel vm)
            {
                // Only auto-submit from the Entry's Completed event when the toggle is ON.
                // (Manual submission is still available via the Submit button.)
                if (vm.AutoSubmitEnabled && vm.SubmitCommand.CanExecute(null))
                    vm.SubmitCommand.Execute(null);
            }

            SerialNumberEntry.Focus(); // Return focus to allow fast repeat input.
        }

        /// <summary>
        /// Event handler for a Submit button click.
        /// Executes the SubmitCommand and refocuses the serial number field.
        /// </summary>
        private void OnSubmitClicked(object sender, EventArgs e)
        {
            if (BindingContext is SubmitViewModel vm && vm.SubmitCommand.CanExecute(null))
                vm.SubmitCommand.Execute(null); // Submit on button press.

            SerialNumberEntry.Focus(); // Prepare UI for next serial scan or input.
        }

        /// <summary>
        /// Overrides the OnAppearing lifecycle event.
        /// Called every time the SubmitPage is navigated to or appears on screen.
        /// Triggers ViewModel.LoadAsync() to refresh lookup data and sets input focus.
        /// </summary>
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is SubmitViewModel vm)
                await vm.LoadAsync(); // Reload board types and skids.

            SerialNumberEntry.Focus(); // Auto-focus for immediate barcode entry.
        }
    }
}
