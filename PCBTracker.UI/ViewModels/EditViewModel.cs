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
    /// ViewModel for the Edit page.
    /// Enables filtering of boards, deleting records by serial number,
    /// applying ship dates to skids, deleting all boards on a skid,
    /// tri-state filtering by Imported, and batch setting IsImported by Ship Date and/or Skid.
    /// </summary>
    public partial class EditViewModel : ObservableObject
    {
        private readonly IBoardService _boardService;

        public EditViewModel(IBoardService boardService)
        {
            _boardService = boardService;

            // Defaults
            DateFrom = DateTime.Today.AddDays(-7);
            DateTo = DateTime.Today;

            // Shipped tri-state
            IsShippedOptions = new ObservableCollection<string>(new[] { "Both", "Shipped", "Not Shipped" });
            SelectedIsShippedOption = IsShippedOptions.First();

            // Imported tri-state (NEW)
            IsImportedOptions = new ObservableCollection<string>(new[] { "Both", "Imported", "Not Imported" });
            SelectedIsImportedOption = IsImportedOptions.First();

            // Imported action defaults
            UseImportDate = false;
            ImportTargetDate = DateTime.Today;
            UseImportSkid = false;
            ImportTargetSkid = null;
            ImportFlag = true; // default to mark as imported
        }

        // -----------------------------
        // Observable Collections
        // -----------------------------
        [ObservableProperty] private ObservableCollection<BoardDto> boards = new();
        [ObservableProperty] private ObservableCollection<string> boardTypes = new();
        [ObservableProperty] private ObservableCollection<SkidDto> skids = new();

        // -----------------------------
        // Filter Inputs
        // -----------------------------
        [ObservableProperty] private string? serialNumberFilter;
        [ObservableProperty] private string? selectedBoardTypeFilter;
        [ObservableProperty] private SkidDto? selectedSkidFilter;

        [ObservableProperty] private DateTime dateFrom;
        [ObservableProperty] private DateTime dateTo;

        // Date mode (same as original)
        [ObservableProperty] private bool useShipDate = false;
        public string DateModeButtonText => UseShipDate ? "Use Prep Dates" : "Use Ship Dates";
        public string DateModeLabel => UseShipDate
            ? "Filtering by Ship Date range."
            : "Filtering by Prep Date range.";

        // Shipped tri-state
        [ObservableProperty] private ObservableCollection<string> isShippedOptions;
        [ObservableProperty] private string selectedIsShippedOption = "Both";

        // Imported tri-state (NEW)
        [ObservableProperty] private ObservableCollection<string> isImportedOptions;
        [ObservableProperty] private string selectedIsImportedOption = "Both";

        // -----------------------------
        // Actions
        // -----------------------------
        // Remove by serial
        [ObservableProperty] private string? removeSerialNumber;

        // Apply ship date to skid
        [ObservableProperty] private SkidDto? applyShipDateSkid;
        [ObservableProperty] private DateTime newShipDate = DateTime.Today;

        // NEW: Set Imported Flag by Ship Date and/or Skid
        [ObservableProperty] private bool useImportDate;
        [ObservableProperty] private DateTime importTargetDate;
        [ObservableProperty] private bool useImportSkid;
        [ObservableProperty] private SkidDto? importTargetSkid;
        [ObservableProperty] private bool importFlag;

        // Delete skid
        [ObservableProperty] private SkidDto? deleteSkidTarget;

        // -----------------------------
        // Pagination (same behavior)
        // -----------------------------
        private const int PageSize = 50;
        [ObservableProperty] private int pageNumber = 1;
        [ObservableProperty] private bool hasNextPage;

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
        public async Task LoadAsync()
        {
            if (!BoardTypes.Any())
            {
                var types = await _boardService.GetBoardTypesAsync();
                BoardTypes = new ObservableCollection<string>(types.Prepend("All"));
                SelectedBoardTypeFilter = BoardTypes.First();
            }

            if (!Skids.Any())
            {
                var skids = await _boardService.ExtractSkidsAsync();
                Skids = new ObservableCollection<SkidDto>(skids.Prepend(new SkidDto { SkidID = 0, SkidName = "All" }));
                SelectedSkidFilter = Skids.First();
            }

            await Search();
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

        [RelayCommand]
        private async Task ExportCsv()
        {
            // Export ALL rows matching current filters (not just the visible page).
            var exportFilter = BuildFilterForAll();
            var allBoards = await _boardService.GetBoardsAsync(exportFilter);

            var sb = new StringBuilder();
            sb.AppendLine("SerialNumber,PartNumber,BoardType,PrepDate,ShipDate,IsShipped,IsImported,SkidID");

            foreach (var b in allBoards)
            {
                var prep = b.PrepDate.ToString("yyyy-MM-dd");
                var ship = b.ShipDate.HasValue ? b.ShipDate.Value.ToString("yyyy-MM-dd") : "";
                sb.AppendLine($"{b.SerialNumber},{b.PartNumber},{b.BoardType},{prep},{ship},{b.IsShipped},{b.IsImported},{b.SkidID}");
            }

            var fileName = $"edit_boards_all_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

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

        [RelayCommand]
        private async Task RemoveBoard()
        {
            if (string.IsNullOrWhiteSpace(RemoveSerialNumber))
            {
                await App.Current.MainPage.DisplayAlert("Missing Serial", "Enter a serial number to remove.", "OK");
                return;
            }

            await _boardService.DeleteBoardBySerialAsync(RemoveSerialNumber.Trim());
            await App.Current.MainPage.DisplayAlert("Removed", $"Board {RemoveSerialNumber} removed (if it existed).", "OK");
            await LoadPageAsync(PageNumber);
        }

        [RelayCommand]
        private async Task ApplyShipDateToSkid()
        {
            if (ApplyShipDateSkid == null || ApplyShipDateSkid.SkidID <= 0)
            {
                await App.Current.MainPage.DisplayAlert("Missing Skid", "Choose a skid.", "OK");
                return;
            }

            await _boardService.UpdateShipDateForSkidAsync(ApplyShipDateSkid.SkidID, NewShipDate);
            await App.Current.MainPage.DisplayAlert("Updated", $"Applied {NewShipDate:yyyy-MM-dd} to skid {ApplyShipDateSkid.SkidName}.", "OK");
            await LoadPageAsync(PageNumber);
        }

        // NEW: Apply imported flag by Ship Date and/or Skid
        [RelayCommand]
        private async Task ApplyImportFlag()
        {
            DateTime? dateCriterion = null;
            bool? useShipDateField = null;

            if (UseImportDate)
            {
                dateCriterion = ImportTargetDate.Date;
                useShipDateField = true; // ALWAYS Ship Date
            }

            int? skidCriterion = null;
            if (UseImportSkid && ImportTargetSkid != null && ImportTargetSkid.SkidID > 0)
            {
                skidCriterion = ImportTargetSkid.SkidID;
            }

            if (dateCriterion == null && skidCriterion == null)
            {
                await App.Current.MainPage.DisplayAlert("No Criteria", "Select at least Date or Skid (or both).", "OK");
                return;
            }

            var affected = await _boardService.UpdateIsImportedAsync(dateCriterion, useShipDateField, skidCriterion, ImportFlag);
            await App.Current.MainPage.DisplayAlert("Done", $"Updated {affected} board(s).", "OK");
            await LoadPageAsync(PageNumber);
        }

        [RelayCommand]
        private async Task DeleteSkid()
        {
            if (DeleteSkidTarget == null || DeleteSkidTarget.SkidID <= 0)
            {
                await App.Current.MainPage.DisplayAlert("Missing Skid", "Choose a skid to delete.", "OK");
                return;
            }

            var count = await _boardService.DeleteBoardsBySkidAsync(DeleteSkidTarget.SkidID);
            await App.Current.MainPage.DisplayAlert("Deleted", $"Deleted {count} board(s) on skid {DeleteSkidTarget.SkidName}.", "OK");
            await LoadPageAsync(PageNumber);
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

        // NEW: unpaged filter for exporting ALL rows matching the current filters
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
            Boards.Clear();

            var filter = BuildFilterPaged(page, PageSize);
            var results = (await _boardService.GetBoardsAsync(filter)).ToList();

            foreach (var b in results)
                Boards.Add(b);

            HasNextPage = results.Count == PageSize;
        }
    }
}
