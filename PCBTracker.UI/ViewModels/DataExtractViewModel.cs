using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PCBTracker.Domain.DTOs;
using PCBTracker.Services.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace PCBTracker.UI.ViewModels
{
    /// <summary>
    /// ViewModel for the DataExtractPage.
    /// Manages filters, paginated board retrieval, CSV export, and total count across all pages.
    /// Uses CommunityToolkit.MVVM for property binding and relay commands.
    /// </summary>
    public partial class DataExtractViewModel : ObservableObject
    {
        private readonly IBoardService _boardService;

        // ------------------------------
        // Results + Lookups
        // ------------------------------

        [ObservableProperty]
        private ObservableCollection<BoardDto> boards = new();

        [ObservableProperty]
        private ObservableCollection<string> boardTypes = new();

        [ObservableProperty]
        private ObservableCollection<SkidDto> skids = new();

        // ------------------------------
        // Filters
        // ------------------------------

        [ObservableProperty]
        private string serialNumberFilter = string.Empty;

        [ObservableProperty]
        private string selectedBoardTypeFilter = string.Empty;

        [ObservableProperty]
        private SkidDto? selectedSkidFilter = null;

        [ObservableProperty]
        private DateTime dateFrom = DateTime.Today.AddMonths(-1);

        [ObservableProperty]
        private DateTime dateTo = DateTime.Today;

        [ObservableProperty]
        private bool useShipDate = false;

        [ObservableProperty]
        private string selectedIsShippedOption = "Both";

        public System.Collections.Generic.List<string> IsShippedOptions { get; } =
            new() { "Both", "Shipped", "Not Shipped" };

        // ------------------------------
        // Paging
        // ------------------------------

        [ObservableProperty]
        private int pageNumber = 1;

        [ObservableProperty]
        private int pageSize = 50;

        [ObservableProperty]
        private bool hasNextPage;

        // ------------------------------
        // Totals
        // ------------------------------

        /// <summary>
        /// Total number of records that match the current filters across ALL pages.
        /// </summary>
        [ObservableProperty]
        private int totalCount;

        /// <summary>
        /// Convenience label if you want to show text instead of just a number.
        /// </summary>
        public string TotalCountLabel => $"Total matching boards: {TotalCount:N0}";

        // ------------------------------
        // UI text
        // ------------------------------

        public string DateModeLabel => useShipDate ? "Filtering by Ship Date" : "Filtering by Prep Date";
        public string DateModeButtonText => useShipDate ? "Switch to Prep Date" : "Switch to Ship Date";

        // ------------------------------
        // Ctor
        // ------------------------------

        public DataExtractViewModel(IBoardService boardService) => _boardService = boardService;

        // ------------------------------
        // Load lookups
        // ------------------------------

        [RelayCommand]
        public async Task LoadAsync()
        {
            var types = await _boardService.GetBoardTypesAsync();
            BoardTypes.Clear();
            BoardTypes.Add(string.Empty); // blank = all
            foreach (var t in types) BoardTypes.Add(t);

            var allSkids = await _boardService.ExtractSkidsAsync();
            Skids.Clear();
            Skids.Add(new SkidDto { SkidID = 0, SkidName = "<All Skids>" });
            foreach (var s in allSkids) Skids.Add(s);

            await SearchAsync();
        }

        // ------------------------------
        // Toggle date mode
        // ------------------------------

        [RelayCommand]
        public void ToggleDateMode()
        {
            UseShipDate = !UseShipDate;
            OnPropertyChanged(nameof(DateModeLabel));
            OnPropertyChanged(nameof(DateModeButtonText));
        }

        // ------------------------------
        // Export CSV (all results, no paging)
        // ------------------------------

        [RelayCommand]
        public async Task ExportCsvAsync()
        {
            bool confirm = await App.Current.MainPage.DisplayAlert(
                "Ready to Convert?",
                "Would you like to export this data to CSV?",
                "Yes",
                "No");

            if (!confirm) return;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("SerialNumber,PartNumber,ShipDate");

            var exportFilter = BuildFilterForAll();
            var allBoards = await _boardService.GetBoardsAsync(exportFilter);

            foreach (var b in allBoards)
            {
                var shipDate = b.ShipDate?.ToString("yyyy-MM-dd") ?? "";
                sb.AppendLine($"{b.SerialNumber},{b.PartNumber},{shipDate}");
            }

            var fileName = $"Boards_{DateTime.Now:yyyyMMddHHmmss}.csv";
            string folderPath;

#if WINDOWS
            folderPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads");
#elif MACCATALYST
            folderPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                "Downloads");
#else
            folderPath = FileSystem.CacheDirectory;
#endif

            var filePath = System.IO.Path.Combine(folderPath, fileName);
            System.IO.File.WriteAllText(filePath, sb.ToString());

            await App.Current.MainPage.DisplayAlert("Export Complete", $"File saved to:\n{filePath}", "OK");
        }

        // ------------------------------
        // Search + Paging
        // ------------------------------

        [RelayCommand]
        public async Task SearchAsync()
        {
            PageNumber = 1;
            await LoadPageAsync(PageNumber);
        }

        [RelayCommand]
        private async Task LoadPageAsync(int page)
        {
            // Build paged filter for visible page
            var pageFilter = BuildFilterPaged(page, PageSize);

            // Fetch current page
            var results = (await _boardService.GetBoardsAsync(pageFilter)).ToList();

            Boards.Clear();
            foreach (var b in results) Boards.Add(b);

            // Preload next page to toggle "Next"
            var nextPageFilter = BuildFilterPaged(page + 1, PageSize);
            var nextPageResults = await _boardService.GetBoardsAsync(nextPageFilter);
            HasNextPage = nextPageResults.Any();

            // Fetch total count across ALL pages (re-query with no paging)
            var countFilter = BuildFilterForAll();
            var allMatching = await _boardService.GetBoardsAsync(countFilter);
            TotalCount = allMatching.Count();
            OnPropertyChanged(nameof(TotalCountLabel));
        }

        [RelayCommand]
        private async Task NextPageAsync()
        {
            if (!HasNextPage) return;
            PageNumber++;
            await LoadPageAsync(PageNumber);
        }

        [RelayCommand]
        private async Task PreviousPageAsync()
        {
            if (PageNumber <= 1) return;
            PageNumber--;
            await LoadPageAsync(PageNumber);
        }

        // ------------------------------
        // Helpers to build filters
        // ------------------------------

        private BoardFilterDto BuildFilterPaged(int page, int size) => new BoardFilterDto
        {
            SerialNumber = SerialNumberFilter,
            BoardType = SelectedBoardTypeFilter,
            SkidId = SelectedSkidFilter?.SkidID > 0 ? SelectedSkidFilter.SkidID : null,
            PrepDateFrom = UseShipDate ? null : DateFrom,
            PrepDateTo = UseShipDate ? null : DateTo,
            ShipDateFrom = UseShipDate ? DateFrom : null,
            ShipDateTo = UseShipDate ? DateTo : null,
            IsShipped = SelectedIsShippedOption switch
            {
                "Shipped" => true,
                "Not Shipped" => false,
                _ => (bool?)null
            },
            PageNumber = page,
            PageSize = size
        };

        private BoardFilterDto BuildFilterForAll() => new BoardFilterDto
        {
            SerialNumber = SerialNumberFilter,
            BoardType = SelectedBoardTypeFilter,
            SkidId = SelectedSkidFilter?.SkidID > 0 ? SelectedSkidFilter.SkidID : null,
            PrepDateFrom = UseShipDate ? null : DateFrom,
            PrepDateTo = UseShipDate ? null : DateTo,
            ShipDateFrom = UseShipDate ? DateFrom : null,
            ShipDateTo = UseShipDate ? DateTo : null,
            IsShipped = SelectedIsShippedOption switch
            {
                "Shipped" => true,
                "Not Shipped" => false,
                _ => (bool?)null
            },
            PageNumber = null,
            PageSize = null
        };
    }
}
