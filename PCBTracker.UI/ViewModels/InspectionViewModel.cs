// InspectionViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PCBTracker.Domain.DTOs;
using PCBTracker.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PCBTracker.UI.ViewModels
{
    public partial class InspectionViewModel : ObservableObject
    {
        private readonly IInspectionService _inspectionService;

        public InspectionViewModel(IInspectionService inspectionService)
        {
            _inspectionService = inspectionService;
            Date = DateTime.Today;
            SeverityLevels = new ObservableCollection<string> { "Minor", "Moderate", "High", "Unknown" };
            ProductTypes = new ObservableCollection<string> { "LE", "LE Upgrade", "SAT", "SAT Upgrade", "SAD", "SAD Upgrade", "Node" }; 
        }

        // Input Fields
        [ObservableProperty] private DateTime date;
        [ObservableProperty] private string productType = string.Empty;
        [ObservableProperty] private string serialNumber = string.Empty;
        [ObservableProperty] private string issueDescription = string.Empty;
        [ObservableProperty] private string severityLevel = "Unknown";
        [ObservableProperty] private string immediateActionTaken = string.Empty;
        [ObservableProperty] private string additionalNotes = string.Empty;

        // Dropdown Sources
        public ObservableCollection<string> ProductTypes { get; }
        public ObservableCollection<string> SeverityLevels { get; }

        // Assemblies
        public ObservableCollection<KeyValuePair<string, int>> AssembliesCompleted { get; } = new();

        public void AddAssembly(string product, int count)
        {
            var existing = AssembliesCompleted.FirstOrDefault(kvp => kvp.Key == product);
            if (!existing.Equals(default(KeyValuePair<string, int>)))
            {
                AssembliesCompleted.Remove(existing);
                AssembliesCompleted.Add(new KeyValuePair<string, int>(product, existing.Value + count));
            }
            else
            {
                AssembliesCompleted.Add(new KeyValuePair<string, int>(product, count));
            }
        }

        // Inspection List
        [ObservableProperty] private ObservableCollection<InspectionDto> inspections = new();

        [RelayCommand]
        public async Task LoadAsync()
        {
            var results = await _inspectionService.GetInspectionsAsync(new InspectionFilterDto
            {
                DateFrom = DateTime.Today.AddMonths(-1),
                DateTo = DateTime.Today
            });

            Inspections.Clear();
            foreach (var i in results)
                Inspections.Add(i);
        }

        [RelayCommand]
        public async Task SubmitAsync()
        {
            var dto = new InspectionDto
            {
                Date = Date,
                ProductType = ProductType,
                SerialNumber = SerialNumber,
                IssueDescription = IssueDescription,
                SeverityLevel = SeverityLevel,
                ImmediateActionTaken = ImmediateActionTaken,
                AdditionalNotes = AdditionalNotes
            };

            await _inspectionService.SubmitInspectionAsync(dto);
            await LoadAsync();

            // Reset form
            ProductType = SerialNumber = IssueDescription = ImmediateActionTaken = AdditionalNotes = string.Empty;
            SeverityLevel = "Unknown";
            Date = DateTime.Today;
            AssembliesCompleted.Clear();
        }

        [RelayCommand]
        public async Task ExportCsvAsync()
        {
            bool confirm = await App.Current.MainPage.DisplayAlert(
                "Export to CSV",
                "Would you like to export these inspection records?",
                "Yes",
                "No");

            if (!confirm) return;

            var sb = new StringBuilder();
            sb.AppendLine("Date,ProductType,SerialNumber,SeverityLevel,IssueDescription,ImmediateActionTaken,AdditionalNotes");

            foreach (var i in Inspections)
            {
                sb.AppendLine($"{i.Date:yyyy-MM-dd},{i.ProductType},{i.SerialNumber},{i.SeverityLevel},\"{i.IssueDescription}\",\"{i.ImmediateActionTaken}\",\"{i.AdditionalNotes}\"");
            }

            var fileName = $"Inspections_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            string folderPath;

#if WINDOWS
            folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
#elif MACCATALYST
            folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Downloads");
#else
            folderPath = FileSystem.CacheDirectory;
#endif

            var filePath = Path.Combine(folderPath, fileName);
            File.WriteAllText(filePath, sb.ToString());

            await App.Current.MainPage.DisplayAlert("Export Complete", $"File saved to:\n{filePath}", "OK");
        }
    }
}