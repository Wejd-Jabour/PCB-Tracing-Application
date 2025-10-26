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
    /// </summary>
    public partial class EditViewModel : ObservableObject
    {
        private readonly IBoardService _boardService;

        public EditViewModel(IBoardService boardService)
        {
            _boardService = boardService;

            DateFrom = DateTime.Today.AddDays(-7);
            DateTo = DateTime.Today;

            IsShippedOptions = new ObservableCollection<string>(new[] { "Both", "Shipped", "Not Shipped" });
            SelectedIsShippedOption = IsShippedOptions.First();

            IsImportedOptions = new ObservableCollection<string>(new[] { "Both", "Imported", "Not Imported" });
            SelectedIsImportedOption = IsImportedOptions.First();

            NewShipDate = DateTime.Today;
            ImportTargetDate = DateTime.Today;
        }

        // Collections
        [ObservableProperty] private ObservableCollection<BoardDto> boards = new();
        [ObservableProperty] private ObservableCollection<string> boardTypes = new();
        [ObservableProperty] private ObservableCollection<SkidDto> skids = new();

        // Filters
        [ObservableProperty] private string? serialNumberFilter;
        [ObservableProperty] private string? selectedBoardTypeFilter;
        [ObservableProperty] private SkidDto? selectedSkidFilter;
        [ObservableProperty] private DateTime dateFrom;
        [ObservableProperty] private DateTime dateTo;

        [ObservableProperty] private bool useShipDate = false;
        public string DateModeButtonText => UseShipDate ? "Use Prep Dates" : "Use Ship Dates";
        public string DateModeLabel => UseShipDate ? "Filtering by Ship Date range." : "Filtering by Prep Date range.";

        // Shipped / Imported tri-state
        [ObservableProperty] private ObservableCollection<string> isShippedOptions = new(new[] { "Both", "Shipped", "Not Shipped" });
        [ObservableProperty] private string selectedIsShippedOption = "Both";

        [ObservableProperty] private ObservableCollection<string> isImportedOptions = new(new[] { "Both", "Imported", "Not Imported" });
        [ObservableProperty] private string selectedIsImportedOption = "Both";

        // Pagination
        private const int PageSize = 50;
        [ObservableProperty] private int pageNumber = 1;
        [ObservableProperty] private bool hasNextPage;

        // Actions UI state
        [ObservableProperty] private string? removeSerialNumber; // <-- for XAML Entry.Text
        [ObservableProperty] private SkidDto? applyShipDateSkid;
        [ObservableProperty] private DateTime newShipDate = DateTime.Today;

        [ObservableProperty] private bool useImportDate;
        [ObservableProperty] private DateTime importTargetDate = DateTime.Today;
        [ObservableProperty] private bool useImportSkid;
        [ObservableProperty] private SkidDto? importTargetSkid;
        [ObservableProperty] private bool importFlag;

        [ObservableProperty] private SkidDto? deleteSkidTarget;

        // NEW: Clear ShipDate for Skid
        [ObservableProperty] private SkidDto? clearShipDateSkid;

        // Commands
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
            // Refresh board types every time; preserve selection if possible
            var previousSelection = SelectedBoardTypeFilter;
            var types = await _boardService.GetBoardTypesAsync();
            BoardTypes = new ObservableCollection<string>(types.Prepend("All"));
            SelectedBoardTypeFilter =
                !string.IsNullOrWhiteSpace(previousSelection) && BoardTypes.Contains(previousSelection)
                    ? previousSelection
                    : BoardTypes.FirstOrDefault();

            // Load skids once
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

        // --- Buttons from XAML: Remove Board ---
        [RelayCommand]
        private async Task RemoveBoard()
        {
            var serial = (RemoveSerialNumber ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(serial))
            {
                await App.Current.MainPage.DisplayAlert("Remove Board", "Enter a serial number.", "OK");
                return;
            }

            var confirm = await App.Current.MainPage.DisplayAlert("Confirm", $"Delete board with Serial '{serial}'?", "Delete", "Cancel");
            if (!confirm) return;

            await _boardService.DeleteBoardBySerialAsync(serial);
            await App.Current.MainPage.DisplayAlert("Deleted", $"Board '{serial}' removed (if it existed).", "OK");

            RemoveSerialNumber = string.Empty;
            await Search();
        }

        // --- Buttons from XAML: Apply Ship Date to Skid ---
        [RelayCommand]
        private async Task ApplyShipDateToSkid()
        {
            if (ApplyShipDateSkid == null || ApplyShipDateSkid.SkidID <= 0)
            {
                await App.Current.MainPage.DisplayAlert("Apply Ship Date", "Select a valid Skid.", "OK");
                return;
            }

            var confirm = await App.Current.MainPage.DisplayAlert(
                "Confirm",
                $"Apply Ship Date {NewShipDate:yyyy-MM-dd} to all boards on Skid '{ApplyShipDateSkid.SkidName}'?",
                "Apply",
                "Cancel");

            if (!confirm) return;

            await _boardService.UpdateShipDateForSkidAsync(ApplyShipDateSkid.SkidID, NewShipDate);
            await App.Current.MainPage.DisplayAlert("Done", "Ship dates updated.", "OK");
            await Search();
        }

        // --- NEW: Clear Ship Date for Skid (also sets IsShipped = false) ---
        [RelayCommand]
        private async Task ClearShipDateForSkid()
        {
            if (ClearShipDateSkid == null || ClearShipDateSkid.SkidID <= 0)
            {
                await App.Current.MainPage.DisplayAlert("Clear Ship Date", "Select a valid Skid.", "OK");
                return;
            }

            var confirm = await App.Current.MainPage.DisplayAlert(
                "Confirm",
                $"Set Ship Date = null and IsShipped = false for ALL boards on Skid '{ClearShipDateSkid.SkidName}' (#{ClearShipDateSkid.SkidID})?",
                "Clear",
                "Cancel");

            if (!confirm) return;

            await _boardService.ClearShipDateForSkidAsync(ClearShipDateSkid.SkidID);
            await App.Current.MainPage.DisplayAlert("Done", "Ship dates cleared and marked Not Shipped.", "OK");
            await Search();
        }

        // --- Buttons from XAML: Apply Imported Flag ---
        [RelayCommand]
        private async Task ApplyImportFlag()
        {
            DateTime? dateCriterion = null;
            bool? useShipDateField = null;

            if (UseImportDate)
            {
                dateCriterion = ImportTargetDate.Date;
                // Using ShipDate as the date field for this action (UI label is "Use Date (Ship Date)")
                useShipDateField = true;
            }

            int? skidCriterion = null;
            if (UseImportSkid)
            {
                if (ImportTargetSkid == null || ImportTargetSkid.SkidID <= 0)
                {
                    await App.Current.MainPage.DisplayAlert("Set Imported Flag", "Select a valid Skid.", "OK");
                    return;
                }
                skidCriterion = ImportTargetSkid.SkidID;
            }

            if (!dateCriterion.HasValue && !skidCriterion.HasValue)
            {
                await App.Current.MainPage.DisplayAlert("Set Imported Flag", "Choose at least one filter: By date and/or By skid.", "OK");
                return;
            }

            var confirm = await App.Current.MainPage.DisplayAlert(
                "Confirm",
                $"Set Imported={(ImportFlag ? "true" : "false")} on boards{(dateCriterion.HasValue ? $" with Ship Date = {dateCriterion:yyyy-MM-dd}" : "")}{(skidCriterion.HasValue ? $" on Skid '{ImportTargetSkid!.SkidName}'" : "")}?",
                "Apply",
                "Cancel");

            if (!confirm) return;

            var affected = await _boardService.UpdateIsImportedAsync(dateCriterion, useShipDateField, skidCriterion, ImportFlag);
            await App.Current.MainPage.DisplayAlert("Done", $"Updated {affected} board(s).", "OK");
            await Search();
        }

        // --- Buttons from XAML: Delete Skid Boards ---
        [RelayCommand]
        private async Task DeleteSkid()
        {
            if (DeleteSkidTarget == null || DeleteSkidTarget.SkidID <= 0)
            {
                await App.Current.MainPage.DisplayAlert("Delete Skid Boards", "Select a valid Skid.", "OK");
                return;
            }

            var confirm = await App.Current.MainPage.DisplayAlert(
                "Confirm",
                $"Delete ALL boards on Skid '{DeleteSkidTarget.SkidName}'? This cannot be undone.",
                "Delete",
                "Cancel");

            if (!confirm) return;

            var count = await _boardService.DeleteBoardsBySkidAsync(DeleteSkidTarget.SkidID);
            await App.Current.MainPage.DisplayAlert("Deleted", $"Removed {count} board(s).", "OK");
            await Search();
        }

        [RelayCommand]
        private async Task ExportCsv()
        {
            var filterAll = BuildFilterForAll();
            var allBoards = await _boardService.GetBoardsAsync(filterAll);

            var sb = new StringBuilder();
            sb.AppendLine("SerialNumber,PartNumber,ShipDate");

            foreach (var b in allBoards)
            {
                var ship = b.ShipDate.HasValue ? b.ShipDate.Value.ToString("yyyy-MM-dd") : "";
                sb.AppendLine($"{b.SerialNumber},{b.PartNumber},{ship}");
            }

            var fileName = $"Edit_Export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

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

        // Helpers
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
            Boards.Clear();

            var filter = BuildFilterPaged(page, PageSize);
            var results = (await _boardService.GetBoardsAsync(filter)).ToList();

            foreach (var b in results)
                Boards.Add(b);

            HasNextPage = results.Count == PageSize;
        }
    }
}
