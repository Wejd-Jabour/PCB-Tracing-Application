using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PCBTracker.Domain.DTOs;
using PCBTracker.Services.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCBTracker.UI.ViewModels
{
    /// <summary>
    /// ViewModel for the DataExtractPage.
    /// Manages filters, paginated board retrieval, CSV export, and total count across all pages.
    /// Adds tri-state Imported filter (Both / Imported / Not Imported) without changing prior paging semantics.
    /// </summary>
    public partial class DataExtractViewModel : ObservableObject
    {
        private readonly IBoardService _boardService;

        public DataExtractViewModel(IBoardService boardService)
        {
            _boardService = boardService;

            // Default date window
            DateFrom = DateTime.Today.AddDays(-7);
            DateTo = DateTime.Today;

            // Shipped tri-state
            IsShippedOptions = new ObservableCollection<string>(new[] { "Both", "Shipped", "Not Shipped" });
            SelectedIsShippedOption = IsShippedOptions.First();

            // Imported tri-state (NEW)
            IsImportedOptions = new ObservableCollection<string>(new[] { "Both", "Imported", "Not Imported" });
            SelectedIsImportedOption = IsImportedOptions.First();
        }

        // -----------------------------
        // Collections displayed in UI
        // -----------------------------
        [ObservableProperty] private ObservableCollection<BoardDto> boards = new();
        [ObservableProperty] private ObservableCollection<string> boardTypes = new();
        [ObservableProperty] private ObservableCollection<SkidDto> skids = new();

        // -----------------------------
        // Filter inputs
        // -----------------------------
        [ObservableProperty] private string? serialNumberFilter;
        [ObservableProperty] private string? selectedBoardTypeFilter;
        [ObservableProperty] private SkidDto? selectedSkidFilter;
        [ObservableProperty] private DateTime dateFrom;
        [ObservableProperty] private DateTime dateTo;

        // Toggle between prep-date or ship-date filtering
        [ObservableProperty] private bool useShipDate = false;

        // Labels / helpers for date mode
        public string DateModeButtonText => UseShipDate ? "Use Prep Dates" : "Use Ship Dates";
        public string DateModeLabel => UseShipDate
            ? "Filtering by Ship Date range."
            : "Filtering by Prep Date range.";

        // Shipped tri-state
        [ObservableProperty] private ObservableCollection<string> isShippedOptions = new(new[] { "Both", "Shipped", "Not Shipped" });
        [ObservableProperty] private string selectedIsShippedOption = "Both";

        // Imported tri-state 
        [ObservableProperty] private ObservableCollection<string> isImportedOptions = new(new[] { "Both", "Imported", "Not Imported" });
        [ObservableProperty] private string selectedIsImportedOption = "Both";

        // -----------------------------
        // Pagination UI state (preserved semantics)
        // -----------------------------
        private const int PageSize = 50;
        [ObservableProperty] private int pageNumber = 1;
        [ObservableProperty] private bool hasNextPage;

        [ObservableProperty] private string totalCountLabel = "Total: 0";
        private int _totalCount = 0;

        // -----------------------------
        // Commands
        // -----------------------------
        [RelayCommand]
        private async Task ToggleDateMode()
        {
            UseShipDate = !UseShipDate;
            OnPropertyChanged(nameof(DateModeButtonText));
            OnPropertyChanged(nameof(DateModeLabel));
        }

        [RelayCommand]
        private async Task Search()
        {
            PageNumber = 1;
            await LoadPageAsync(PageNumber);
        }

        [RelayCommand]
        private async Task NextPage()
        {
            if (!HasNextPage) return;
            PageNumber++;
            await LoadPageAsync(PageNumber);
        }

        [RelayCommand]
        private async Task PreviousPage()
        {
            if (PageNumber <= 1) return;
            PageNumber--;
            await LoadPageAsync(PageNumber);
        }

        // -----------------------------
        // FIXED EXPORT: three columns, all filtered rows (no paging)
        // -----------------------------
        [RelayCommand]
        private async Task ExportCsv()
        {
            // Build an unpaged filter so we export ALL rows that match the current filters.
            var exportFilter = BuildFilterForAll();
            var allBoards = await _boardService.GetBoardsAsync(exportFilter);

            var sb = new StringBuilder();
            sb.AppendLine("SerialNumber,PartNumber,ShipDate");

            foreach (var b in allBoards)
            {
                var ship = b.ShipDate.HasValue ? b.ShipDate.Value.ToString("yyyy-MM-dd") : "";
                sb.AppendLine($"{b.SerialNumber},{b.PartNumber},{ship}");
            }

            var fileName = $"Boards_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

#if WINDOWS
            var folderPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads");
#elif MACCATALYST
            var folderPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                "Downloads");
#else
            var folderPath = FileSystem.CacheDirectory;
#endif

            var filePath = System.IO.Path.Combine(folderPath, fileName);
            System.IO.File.WriteAllText(filePath, sb.ToString());

            await App.Current.MainPage.DisplayAlert("Export Complete", $"Saved to {filePath}", "OK");
        }

        // -----------------------------
        // Public lifecycle
        // -----------------------------
        public async Task LoadAsync()
        {
            if (!BoardTypes.Any())
            {
                var types = await _boardService.GetBoardTypesAsync();
                BoardTypes = new ObservableCollection<string>(types.Prepend("All"));
                SelectedBoardTypeFilter = BoardTypes.FirstOrDefault();
            }

            if (!Skids.Any())
            {
                var skids = await _boardService.ExtractSkidsAsync();
                Skids = new ObservableCollection<SkidDto>(skids.Prepend(new SkidDto { SkidID = 0, SkidName = "All" }));
                SelectedSkidFilter = Skids.FirstOrDefault();
            }

            await Search();
        }

        // -----------------------------
        // Internal helpers
        // -----------------------------
        private BoardFilterDto BuildFilterPaged(int page, int size) => new BoardFilterDto
        {
            SerialNumber = SerialNumberFilter,
            BoardType = SelectedBoardTypeFilter != null && SelectedBoardTypeFilter != "All" ? SelectedBoardTypeFilter : null,
            SkidId = (SelectedSkidFilter?.SkidID ?? 0) > 0 ? SelectedSkidFilter!.SkidID : (int?)null,
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
            IsImported = SelectedIsImportedOption switch
            {
                "Imported" => true,
                "Not Imported" => false,
                _ => (bool?)null
            },
            PageNumber = page,
            PageSize = size
        };

        private BoardFilterDto BuildFilterForAll() => new BoardFilterDto
        {
            SerialNumber = SerialNumberFilter,
            BoardType = SelectedBoardTypeFilter != null && SelectedBoardTypeFilter != "All" ? SelectedBoardTypeFilter : null,
            SkidId = (SelectedSkidFilter?.SkidID ?? 0) > 0 ? SelectedSkidFilter!.SkidID : (int?)null,
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
            IsImported = SelectedIsImportedOption switch
            {
                "Imported" => true,
                "Not Imported" => false,
                _ => (bool?)null
            },
            PageNumber = null,
            PageSize = null
        };

        private async Task LoadPageAsync(int page)
        {
            var allFilter = BuildFilterForAll();
            var allResults = await _boardService.GetBoardsAsync(allFilter);
            _totalCount = allResults.Count();
            TotalCountLabel = $"Total: {_totalCount}";

            var pagedFilter = BuildFilterPaged(page, PageSize);
            var pageResults = (await _boardService.GetBoardsAsync(pagedFilter)).ToList();

            Boards.Clear();
            foreach (var b in pageResults)
                Boards.Add(b);

            HasNextPage = page * PageSize < _totalCount;
        }
    }
}