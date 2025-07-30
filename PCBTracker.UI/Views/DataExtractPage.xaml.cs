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

            BindingContext = Application.Current
                             .Handler
                             .MauiContext
                             .Services
                             .GetRequiredService<DataExtractViewModel>();
        }

        private void OnFilterCompleted(object sender, EventArgs e)
        {
            if (BindingContext is DataExtractViewModel vm &&
                vm.SearchCommand.CanExecute(null))
            {
                vm.SearchCommand.Execute(null);
            }
        }

        private void OnFilterChanged(object sender, EventArgs e)
        {
            if (BindingContext is DataExtractViewModel vm &&
                vm.SearchCommand.CanExecute(null))
            {
                vm.SearchCommand.Execute(null);
            }
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
                await vm.LoadAsync();
        }
    }
}
