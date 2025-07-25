using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using PCBTracker.UI.ViewModels;

namespace PCBTracker.UI.Views
{
    /// <summary>
    /// Code-behind for the EditPage view.
    /// Responsible for initializing the UI components, setting the data context,
    /// and handling page lifecycle events.
    /// </summary>
    public partial class EditPage : ContentPage
    {
        /// <summary>
        /// Constructor for EditPage.
        /// Initializes the visual tree and binds the EditViewModel using MAUI's DI container.
        /// </summary>
        public EditPage()
        {
            // Initializes all XAML-defined UI elements and named controls.
            InitializeComponent();

            // Resolves the EditViewModel from the service provider.
            // This uses MAUI’s dependency injection system configured in MauiProgram.cs.
            BindingContext = Application.Current           // Gets the current MAUI Application instance.
                .Handler                                   // Accesses the platform-specific handler.
                .MauiContext                               // Provides platform-aware service context.
                .Services                                  // The IServiceProvider configured at app startup.
                .GetRequiredService<EditViewModel>();      // Resolves EditViewModel or throws if not registered.
        }

        /// <summary>
        /// Lifecycle method triggered automatically whenever this page becomes visible.
        /// Reinvokes the ViewModel’s LoadAsync() to ensure up-to-date data.
        /// </summary>
        protected override async void OnAppearing()
        {
            // Executes base class OnAppearing logic first (safe override practice).
            base.OnAppearing();

            // If the bound ViewModel is the expected type, trigger initial load logic.
            if (BindingContext is EditViewModel vm)
                await vm.LoadAsync(); // Loads board types, skids, and performs initial search.
        }
    }
}
