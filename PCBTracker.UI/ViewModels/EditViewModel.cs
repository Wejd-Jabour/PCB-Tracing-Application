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
    public partial class EditViewModel : ObservableObject
    {
        private readonly IBoardService _boardService;

        [ObservableProperty] private ObservableCollection<BoardDto> boards = new();
        [ObservableProperty] private ObservableCollection<string> boardTypes = new();
        [ObservableProperty] private ObservableCollection<SkidDto> skids = new();

        [ObservableProperty] private string serialNumberFilter = string.Empty;
        [ObservableProperty] private string selectedBoardTypeFilter = string.Empty;
        [ObservableProperty] private SkidDto? selectedSkidFilter = null;
        [ObservableProperty] private DateTime dateFrom = DateTime.Today.AddMonths(-1);
        [ObservableProperty] private DateTime dateTo = DateTime.Today;
        [ObservableProperty] private bool useShipDate = false;

        [ObservableProperty] private string removeSerialNumber = string.Empty;
        [ObservableProperty] private SkidDto? applyShipDateSkid = null;
        [ObservableProperty] private DateTime newShipDate = DateTime.Today;

        [ObservableProperty] private int pageNumber = 1;
        [ObservableProperty] private bool hasNextPage;

        public string DateModeLabel => useShipDate ? "Filtering by Ship Date" : "Filtering by Prep Date";
        public string DateModeButtonText => useShipDate ? "Switch to Prep Date" : "Switch to Ship Date";

        public EditViewModel(IBoardService boardService) => _boardService = boardService;

        [RelayCommand]
        public async Task LoadAsync()
        {
            var types = await _boardService.GetBoardTypesAsync();
            BoardTypes.Clear();
            BoardTypes.Add(string.Empty);
            foreach (var t in types)
                BoardTypes.Add(t);

            var allSkids = await _boardService.ExtractSkidsAsync();
            Skids.Clear();
            Skids.Add(new SkidDto { SkidID = 0, SkidName = "<All Skids>" });
            foreach (var s in allSkids)
                Skids.Add(s);

            await SearchAsync();
        }

        [RelayCommand]
        public void ToggleDateMode()
        {
            UseShipDate = !UseShipDate;
            OnPropertyChanged(nameof(DateModeLabel));
            OnPropertyChanged(nameof(DateModeButtonText));
        }

        [RelayCommand]
        public async Task SearchAsync()
        {
            PageNumber = 1;
            await LoadPageAsync(PageNumber);
        }

        [RelayCommand]
        public async Task RemoveBoardAsync()
        {
            if (string.IsNullOrWhiteSpace(RemoveSerialNumber)) return;

            await _boardService.DeleteBoardBySerialAsync(RemoveSerialNumber);
            await SearchAsync();

            await App.Current.MainPage.DisplayAlert("Removed", $"Board {RemoveSerialNumber} deleted.", "OK");
        }

        [RelayCommand]
        public async Task ApplyShipDateToSkidAsync()
        {
            if (ApplyShipDateSkid == null || ApplyShipDateSkid.SkidID == 0) return;

            await _boardService.UpdateShipDateForSkidAsync(ApplyShipDateSkid.SkidID, NewShipDate);
            await SearchAsync();

            await App.Current.MainPage.DisplayAlert("Updated", $"Applied ship date to Skid {ApplyShipDateSkid.SkidName}.", "OK");
        }

        [RelayCommand]
        private async Task ExportCsvAsync()
        {
            bool confirm = await App.Current.MainPage.DisplayAlert(
                "Ready to Export?",
                "Would you like to export this data to CSV?",
                "Yes",
                "No");

            if (!confirm) return;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("SerialNumber,PartNumber,BoardType,PrepDate,ShipDate,IsShipped,SkidID");

            foreach (var b in Boards)
            {
                var shipDate = b.ShipDate?.ToString("yyyy-MM-dd") ?? "";
                sb.AppendLine($"{b.SerialNumber},{b.PartNumber},{b.BoardType},{b.PrepDate:yyyy-MM-dd},{shipDate},{b.IsShipped},{b.SkidID}");
            }

            var fileName = $"EditExport_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            string folderPath;

#if WINDOWS
            folderPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
#else
            folderPath = FileSystem.CacheDirectory;
#endif

            var filePath = System.IO.Path.Combine(folderPath, fileName);
            System.IO.File.WriteAllText(filePath, sb.ToString());

            await App.Current.MainPage.DisplayAlert("Export Complete", $"File saved to:\n{filePath}", "OK");
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
            if (PageNumber > 1)
            {
                PageNumber--;
                await LoadPageAsync(PageNumber);
            }
        }

        private async Task LoadPageAsync(int page)
        {
            var filter = new BoardFilterDto
            {
                SerialNumber = SerialNumberFilter,
                BoardType = SelectedBoardTypeFilter,
                SkidId = SelectedSkidFilter?.SkidID > 0 ? SelectedSkidFilter.SkidID : null,
                PrepDateFrom = UseShipDate ? null : DateFrom,
                PrepDateTo = UseShipDate ? null : DateTo,
                ShipDateFrom = UseShipDate ? DateFrom : null,
                ShipDateTo = UseShipDate ? DateTo : null,
                PageNumber = page,
                PageSize = 50
            };

            var results = (await _boardService.GetBoardsAsync(filter)).ToList();

            Boards.Clear();
            foreach (var b in results)
                Boards.Add(b);

            var nextFilter = new BoardFilterDto
            {
                SerialNumber = filter.SerialNumber,
                BoardType = filter.BoardType,
                SkidId = filter.SkidId,
                PrepDateFrom = filter.PrepDateFrom,
                PrepDateTo = filter.PrepDateTo,
                ShipDateFrom = filter.ShipDateFrom,
                ShipDateTo = filter.ShipDateTo,
                PageNumber = page + 1,
                PageSize = filter.PageSize
            };

            var nextPageResults = await _boardService.GetBoardsAsync(nextFilter);
            HasNextPage = nextPageResults.Any();
        }
    }
}
