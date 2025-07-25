using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using PCBTracker.UI.ViewModels;

namespace PCBTracker.UI.Views
{
    /// <summary>
    /// Code-behind for the DataExtractPage view.
    /// Responsible for resolving the ViewModel, initializing UI components, 
    /// and handling lifecycle behavior like loading data on appearance.
    /// </summary>
    public partial class DataExtractPage : ContentPage
    {
        /// <summary>
        /// Constructor for the DataExtractPage.
        /// Initializes the visual tree and binds the ViewModel to the page.
        /// </summary>
        public DataExtractPage()
        {
            // Loads and instantiates the visual elements defined in DataExtractPage.xaml.
            InitializeComponent();

            // Resolves the DataExtractViewModel from the application's MAUI dependency injection system.
            // The resolved instance is assigned as the BindingContext so that XAML data bindings function correctly.
            BindingContext = Application.Current              // Gets the current Application instance.
                             .Handler                         // Accesses the native platform handler.
                             .MauiContext                     // Provides platform-aware context including services.
                             .Services                        // The service provider container (IServiceProvider).
                             .GetRequiredService<DataExtractViewModel>(); // Resolves the registered ViewModel instance.
        }

        /// <summary>
        /// Called by the framework whenever the page becomes visible (navigated to or restored).
        /// Invokes ViewModel.LoadAsync() to fetch and prepare lookup/filter data.
        /// </summary>
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Ensures the bound ViewModel is the correct type before invoking async logic.
            if (BindingContext is DataExtractViewModel vm)
                await vm.LoadAsync(); // Loads board types, skids, and initiates default search.
        }
    }
}
