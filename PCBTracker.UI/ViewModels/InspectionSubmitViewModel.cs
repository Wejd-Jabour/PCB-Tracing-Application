using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PCBTracker.Domain.DTOs;
using PCBTracker.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Store;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PCBTracker.UI.ViewModels
{
    public partial class InspectionSubmitViewModel : ObservableObject
    {
        private readonly IInspectionService _inspectionService;

        public InspectionSubmitViewModel(IInspectionService inspectionService)
        {
            _inspectionService = inspectionService;
            Date = DateTime.Today;
            SeverityLevels = new ObservableCollection<string> { "Minor", "Moderate", "High", "Unknown" };
            ProductTypes = new ObservableCollection<string> { "LE", "LE Upgrade", "SAD", "SAD Upgrade", "SAT", "SAT Upgrade", "Node" };
        }

        // -----------------------------
        // Bindable Form Fields
        // -----------------------------

        [ObservableProperty]
        private DateTime date;

        [ObservableProperty]
        private string productType;

        [ObservableProperty]
        private string serialNumber;

        [ObservableProperty]
        private string issueDescription;

        [ObservableProperty]
        private string severityLevel;

        [ObservableProperty]
        private string immediateActionTaken;

        [ObservableProperty]
        private string additionalNotes;

        [ObservableProperty]
        private ObservableCollection<string> severityLevels;

        [ObservableProperty]
        private ObservableCollection<string> productTypes;

        // -----------------------------
        // Assemblies Completed
        // -----------------------------

        public ObservableCollection<KeyValuePair<string, int>> AssembliesCompleted { get; set; } = new();

        // Add method for UI button to call
        public void AddAssembly(string product, int count)
        {
            if (!string.IsNullOrWhiteSpace(product) && count > 0)
                AssembliesCompleted.Add(new KeyValuePair<string, int>(product, count));
        }

        // -----------------------------
        // Submit Command
        // -----------------------------

        [RelayCommand]
        private async Task SubmitAsync()
        {
            var dto = new InspectionDto
            {
                Date = Date,
                ProductType = ProductType,
                SerialNumber = SerialNumber,
                IssueDescription = IssueDescription,
                SeverityLevel = SeverityLevel,
                ImmediateActionTaken = ImmediateActionTaken,
                AdditionalNotes = AdditionalNotes,
                AssembliesCompleted = AssembliesCompleted.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            };

            await _inspectionService.SubmitInspectionAsync(dto);

            await App.Current.MainPage.DisplayAlert("Success", "Inspection submitted.", "OK");

            // Optionally reset form
            SerialNumber = string.Empty;
            IssueDescription = string.Empty;
            ImmediateActionTaken = string.Empty;
            AdditionalNotes = string.Empty;
            AssembliesCompleted.Clear();
        }
    }
}
