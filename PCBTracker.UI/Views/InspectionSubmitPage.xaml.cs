using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using PCBTracker.UI.ViewModels;

namespace PCBTracker.UI.Views
{
    public partial class InspectionSubmitPage : ContentPage
    {
        public InspectionSubmitPage()
        {
            InitializeComponent();
            BindingContext = Application.Current
                .Handler.MauiContext.Services
                .GetRequiredService<InspectionSubmitViewModel>();
        }

        private void OnAddAssemblyClicked(object sender, EventArgs e)
        {
            if (BindingContext is InspectionSubmitViewModel vm)
            {
                var product = AssemblyProductInput.Text?.Trim();
                if (int.TryParse(AssemblyCountInput.Text, out int count) && !string.IsNullOrEmpty(product))
                {
                    vm.AddAssembly(product, count); 
                    AssemblyProductInput.Text = "";
                    AssemblyCountInput.Text = "";
                }
            }
        }

    }
}
