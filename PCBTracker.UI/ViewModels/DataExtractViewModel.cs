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
        public async Task SearchAsync()
        {
            var filter = new BoardFilterDto
            {
                SerialNumber = SerialNumberFilter,
                BoardType = SelectedBoardTypeFilter,
                SkidId = SelectedSkidFilter?.SkidID > 0 ? SelectedSkidFilter.SkidID : null
            };

            if (UseShipDate)
            {
                filter.ShipDateFrom = DateFrom;
                filter.ShipDateTo = DateTo;
            }
            else
            {
                filter.PrepDateFrom = DateFrom;
                filter.PrepDateTo = DateTo;
            }

            Boards.Clear();
            var results = await _boardService.GetBoardsAsync(filter);
            foreach (var b in results)
                Boards.Add(b);
        }

        [RelayCommand]
        public async Task ExportCsvAsync()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("SerialNumber,PartNumber,BoardType,PrepDate,ShipDate,IsShipped,SkidID");
            foreach (var b in Boards)
                sb.AppendLine($"{b.SerialNumber},{b.PartNumber},{b.BoardType},{b.PrepDate:yyyy-MM-dd},{b.ShipDate:yyyy-MM-dd},{b.IsShipped},{b.SkidID}");

            var file = System.IO.Path.Combine(FileSystem.CacheDirectory, $"Boards_{DateTime.Now:yyyyMMddHHmmss}.csv");
            System.IO.File.WriteAllText(file, sb.ToString());

            await Share.RequestAsync(new ShareFileRequest("Exported Boards", new ShareFile(file)));
        }
    }
}
