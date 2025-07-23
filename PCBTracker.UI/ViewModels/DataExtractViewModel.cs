using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PCBTracker.Domain.DTOs;
using PCBTracker.Domain.Entities;
using PCBTracker.Services.Interfaces;
using System.Collections.ObjectModel;

namespace PCBTracker.UI.ViewModels
{
    public partial class DataExtractViewModel : ObservableObject
    {
        private readonly IBoardService _boardService;

        [ObservableProperty]
        private ObservableCollection<BoardDto> boards = new();

        [ObservableProperty]
        private ObservableCollection<string> boardTypes = new();

        [ObservableProperty]
        private string serialNumberFilter = string.Empty;

        [ObservableProperty]
        private string selectedBoardTypeFilter = string.Empty;

        [ObservableProperty]
        private DateTime prepDateFrom = DateTime.Today.AddMonths(-1);

        [ObservableProperty]
        private DateTime prepDateTo = DateTime.Today;

        [ObservableProperty]
        private DateTime? shipDateFrom = null;

        [ObservableProperty]
        private DateTime? shipDateTo = null;

        public DataExtractViewModel(IBoardService boardService)
        {
            _boardService = boardService;
        }

        [RelayCommand]
        public async Task LoadAsync()
        {
            // load types for filter
            var types = await _boardService.GetBoardTypesAsync();
            BoardTypes.Clear();
            BoardTypes.Add(string.Empty); // “All”
            foreach (var t in types) BoardTypes.Add(t);

            // initial load
            await SearchAsync();
        }

        [RelayCommand]
        public async Task SearchAsync()
        {
            var filter = new BoardFilterDto
            {
                SerialNumber = SerialNumberFilter,
                BoardType = SelectedBoardTypeFilter,
                PrepDateFrom = PrepDateFrom,
                PrepDateTo = PrepDateTo,
                ShipDateFrom = ShipDateFrom,
                ShipDateTo = ShipDateTo
            };

            Boards.Clear();
            var results = await _boardService.GetBoardsAsync(filter);
            foreach (var b in results) Boards.Add(b);
        }

        [RelayCommand]
        public async Task ExportCsvAsync()
        {
            // simple CSV export to cache + share
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("SerialNumber,PartNumber,BoardType,PrepDate,ShipDate,IsShipped,SkidID");
            foreach (var b in Boards)
            {
                sb.AppendLine($"{b.SerialNumber},{b.PartNumber},{b.BoardType},{b.PrepDate:yyyy-MM-dd},{b.ShipDate:yyyy-MM-dd},{b.IsShipped},{b.SkidID}");
            }

            var file = System.IO.Path.Combine(
                FileSystem.CacheDirectory,
                $"Boards_{DateTime.Now:yyyyMMddHHmmss}.csv");
            System.IO.File.WriteAllText(file, sb.ToString());

            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Exported Boards",
                File = new ShareFile(file)
            });
        }
    }
}
