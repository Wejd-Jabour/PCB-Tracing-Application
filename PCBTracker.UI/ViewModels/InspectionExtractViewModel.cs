using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PCBTracker.Domain.DTOs;
using PCBTracker.Services.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Store;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PCBTracker.UI.ViewModels
{
    public partial class InspectionExtractViewModel : ObservableObject
    {
        private readonly IInspectionService _inspectionService;

        public InspectionExtractViewModel(IInspectionService inspectionService)
        {
            _inspectionService = inspectionService;

            ProductTypes = new ObservableCollection<string> { "", "LE", "LE Upgrade", "SAD", "SAD Upgrade", "SAT", "SAT Upgrade", "node" };
            SeverityLevels = new ObservableCollection<string> { "", "Minor", "Moderate", "High", "Unknown" };
            DateFrom = DateTime.Today.AddMonths(-1);
            DateTo = DateTime.Today;
        }

        // -----------------------------
        // Filter Fields
        // -----------------------------

        [ObservableProperty] private string serialNumberFilter = string.Empty;
        [ObservableProperty] private string selectedProductType = string.Empty;
        [ObservableProperty] private string selectedSeverity = string.Empty;
        [ObservableProperty] private DateTime dateFrom;
        [ObservableProperty] private DateTime dateTo;
        [ObservableProperty] private ObservableCollection<string> productTypes;
        [ObservableProperty] private ObservableCollection<string> severityLevels;

        // -----------------------------
        // Result Set and Paging
        // -----------------------------

        [ObservableProperty] private ObservableCollection<InspectionDto> inspections = new();
        [ObservableProperty] private int pageNumber = 1;
        [ObservableProperty] private int pageSize = 50;
        [ObservableProperty] private bool hasNextPage;

        // -----------------------------
        // Load and Filter
        // -----------------------------

        [RelayCommand]
        public async Task LoadAsync()
        {
            await SearchAsync();
        }

        [RelayCommand]
        public async Task SearchAsync()
        {
            PageNumber = 1;
            await LoadPageAsync(PageNumber);
        }

        private async Task LoadPageAsync(int page)
        {
            var filter = new InspectionFilterDto
            {
                SerialNumberContains = SerialNumberFilter,
                ProductType = SelectedProductType,
                SeverityLevel = SelectedSeverity,
                DateFrom = DateFrom,
                DateTo = DateTo,
                PageNumber = page,
                PageSize = PageSize
            };

            var results = (await _inspectionService.GetInspectionsAsync(filter)).ToList();
            Inspections.Clear();
            foreach (var r in results)
                Inspections.Add(r);

            // Check if there’s a next page
            var nextFilter = new InspectionFilterDto
            {
                SerialNumberContains = SerialNumberFilter,
                ProductType = SelectedProductType,
                SeverityLevel = SelectedSeverity,
                DateFrom = DateFrom,
                DateTo = DateTo,
                PageNumber = page + 1,
                PageSize = PageSize
            };

            var nextPage = await _inspectionService.GetInspectionsAsync(nextFilter);
            HasNextPage = nextPage.Any();
        }

        [RelayCommand]
        public async Task NextPageAsync()
        {
            if (!HasNextPage) return;
            PageNumber++;
            await LoadPageAsync(PageNumber);
        }

        [RelayCommand]
        public async Task PreviousPageAsync()
        {
            if (PageNumber > 1)
            {
                PageNumber--;
                await LoadPageAsync(PageNumber);
            }
        }

        // -----------------------------
        // Export
        // -----------------------------

        [RelayCommand]
        public async Task ExportCsvAsync()
        {
            bool confirm = await App.Current.MainPage.DisplayAlert("Export", "Export to CSV?", "Yes", "No");
            if (!confirm) return;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Date,ProductType,SerialNumber,SeverityLevel,IssueDescription,ImmediateActionTaken,AdditionalNotes");

            foreach (var i in Inspections)
            {
                sb.AppendLine($"\"{i.Date:yyyy-MM-dd}\",\"{i.ProductType}\",\"{i.SerialNumber}\",\"{i.SeverityLevel}\",\"{i.IssueDescription}\",\"{i.ImmediateActionTaken}\",\"{i.AdditionalNotes}\"");
            }

            var fileName = $"Inspections_{DateTime.Now:yyyyMMddHHmmss}.csv";

#if WINDOWS
            string folderPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
#else
            string folderPath = FileSystem.CacheDirectory;
#endif
            string filePath = System.IO.Path.Combine(folderPath, fileName);
            System.IO.File.WriteAllText(filePath, sb.ToString());

            await App.Current.MainPage.DisplayAlert("Exported", $"Saved to:\n{filePath}", "OK");
        }
    }
}
