using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PCBTracker.Domain.DTOs;
using PCBTracker.Services.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace PCBTracker.UI.ViewModels
{
    public partial class DataExtractViewModel : ObservableObject
    {
        private readonly IBoardService _boardService;

        [ObservableProperty]
        ObservableCollection<BoardDto> boards = new();

        [ObservableProperty]
        ObservableCollection<string> boardTypes = new();

        [ObservableProperty]
        ObservableCollection<SkidDto> skids = new();

        [ObservableProperty]
        string serialNumberFilter = string.Empty;

        [ObservableProperty]
        string selectedBoardTypeFilter = string.Empty;

        [ObservableProperty]
        SkidDto? selectedSkidFilter = null;

        [ObservableProperty]
        DateTime dateFrom = DateTime.Today.AddMonths(-1);

        [ObservableProperty]
        DateTime dateTo = DateTime.Today;

        [ObservableProperty]
        bool useShipDate = false;

        [ObservableProperty]
        private int pageNumber = 1;

        [ObservableProperty]
        private int pageSize = 50;

        [ObservableProperty]
        private bool hasNextPage;


        public string DateModeLabel => useShipDate ? "Filtering by Ship Date" : "Filtering by Prep Date";
        public string DateModeButtonText => useShipDate ? "Switch to Prep Date" : "Switch to Ship Date";

        public DataExtractViewModel(IBoardService boardService)
            => _boardService = boardService;

        [RelayCommand]
        public async Task LoadAsync()
        {
            // Load Board Types
            var types = await _boardService.GetBoardTypesAsync();
            BoardTypes.Clear();
            BoardTypes.Add(string.Empty);
            foreach (var t in types) BoardTypes.Add(t);

            // Load Skids via the new DTO method
            var allSkids = await _boardService.ExtractSkidsAsync();
            Skids.Clear();
            Skids.Add(new SkidDto { SkidID = 0, SkidName = "<All Skids>" });
            foreach (var s in allSkids) Skids.Add(s);

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
        public async Task ExportCsvAsync()
        {
            bool confirm = await App.Current.MainPage.DisplayAlert(
                "Ready to Convert?",
                "Would you like to export this data to CSV?",
                "Yes",
                "No");

            if (!confirm) 
                return;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("SerialNumber,PartNumber,ShipDate");

            foreach (var b in Boards)
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
    // Mobile fallback
    folderPath = FileSystem.CacheDirectory;
#endif

            var filePath = System.IO.Path.Combine(folderPath, fileName);
            System.IO.File.WriteAllText(filePath, sb.ToString());

            await App.Current.MainPage.DisplayAlert("Export Complete", $"File saved to:\n{filePath}", "OK");
        }


        [RelayCommand]
        public async Task SearchAsync()
        {
            PageNumber = 1;
            await LoadPageAsync(PageNumber);
        }

        [RelayCommand]
        private async Task LoadPageAsync(int page)
        {
            var filter = new BoardFilterDto
            {
                SerialNumber = SerialNumberFilter,
                BoardType = SelectedBoardTypeFilter,
                SkidId = SelectedSkidFilter?.SkidID > 0 ? SelectedSkidFilter.SkidID : null,
                PageNumber = page,
                PageSize = PageSize,
                PrepDateFrom = UseShipDate ? null : DateFrom,
                PrepDateTo = UseShipDate ? null : DateTo,
                ShipDateFrom = UseShipDate ? DateFrom : null,
                ShipDateTo = UseShipDate ? DateTo : null
            };

            var results = (await _boardService.GetBoardsAsync(filter)).ToList();

            Boards.Clear();
            foreach (var b in results)
                Boards.Add(b);

            // Update HasNextPage
            var nextPageFilter = new BoardFilterDto
            {
                SerialNumber = SerialNumberFilter,
                BoardType = SelectedBoardTypeFilter,
                SkidId = SelectedSkidFilter?.SkidID > 0 ? SelectedSkidFilter.SkidID : null,
                PageNumber = page + 1,
                PageSize = PageSize,
                PrepDateFrom = UseShipDate ? null : DateFrom,
                PrepDateTo = UseShipDate ? null : DateTo,
                ShipDateFrom = UseShipDate ? DateFrom : null,
                ShipDateTo = UseShipDate ? DateTo : null
            };

            var nextPageResults = await _boardService.GetBoardsAsync(nextPageFilter);
            HasNextPage = nextPageResults.Any();
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


    }
}
