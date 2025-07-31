﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PCBTracker.Domain.DTOs;
using PCBTracker.Services.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace PCBTracker.UI.ViewModels
{
    /// <summary>
    /// ViewModel for the DataExtractPage.
    /// Manages filters, paginated board retrieval, and CSV export.
    /// Uses CommunityToolkit.MVVM for property binding and relay commands.
    /// </summary>
    public partial class DataExtractViewModel : ObservableObject
    {
        private readonly IBoardService _boardService;

        /// <summary>
        /// Collection of BoardDto items matching the active filters.
        /// Bound to a ListView or equivalent in the extract page UI.
        /// </summary>
        [ObservableProperty]
        ObservableCollection<BoardDto> boards = new();

        /// <summary>
        /// List of available board types for filtering.
        /// Fetched from IBoardService and bound to a dropdown or picker.
        /// </summary>
        [ObservableProperty]
        ObservableCollection<string> boardTypes = new();

        /// <summary>
        /// List of available skids for filtering.
        /// Includes special entry with ID 0 representing "All Skids".
        /// </summary>
        [ObservableProperty]
        ObservableCollection<SkidDto> skids = new();

        /// <summary>
        /// Optional filter value for matching SerialNumber.
        /// Used in substring matching within search queries.
        /// </summary>
        [ObservableProperty]
        string serialNumberFilter = string.Empty;

        /// <summary>
        /// Currently selected board type filter value.
        /// Matches boards with the specified type exactly.
        /// </summary>
        [ObservableProperty]
        string selectedBoardTypeFilter = string.Empty;

        /// <summary>
        /// Skid selected for filtering; if null or SkidID is 0, all skids are included.
        /// </summary>
        [ObservableProperty]
        SkidDto? selectedSkidFilter = null;

        /// <summary>
        /// Start date of the filter range.
        /// Interpreted as PrepDateFrom or ShipDateFrom depending on UseShipDate.
        /// </summary>
        [ObservableProperty]
        DateTime dateFrom = DateTime.Today.AddMonths(-1);

        [ObservableProperty]
        bool isShipped = false;

        [ObservableProperty]
        private string selectedIsShippedOption = "Both";
        public List<string> IsShippedOptions { get; } = new() { "Both", "Shipped", "Not Shipped" };


        /// <summary>
        /// End date of the filter range.
        /// Interpreted as PrepDateTo or ShipDateTo depending on UseShipDate.
        /// </summary>
        [ObservableProperty]
        DateTime dateTo = DateTime.Today;

        /// <summary>
        /// If true, filters use ShipDate; if false, filters use PrepDate.
        /// </summary>
        [ObservableProperty]
        bool useShipDate = false;

        [ObservableProperty]
        private int pageNumber = 1;

        [ObservableProperty]
        private int pageSize = 50;

        [ObservableProperty]
        private bool hasNextPage;

        /// <summary>
        /// Label for display reflecting the current date filter mode.
        /// </summary>
        public string DateModeLabel => useShipDate ? "Filtering by Ship Date" : "Filtering by Prep Date";

        /// <summary>
        /// Button text shown to allow toggling the date filter mode.
        /// </summary>
        public string DateModeButtonText => useShipDate ? "Switch to Prep Date" : "Switch to Ship Date";

        /// <summary>
        /// Constructor that stores a reference to the board service.
        /// </summary>
        public DataExtractViewModel(IBoardService boardService)
            => _boardService = boardService;

        // ------------------------------
        // Load Command
        // ------------------------------

        /// <summary>
        /// Loads available board types and skids.
        /// Initializes Skid filter with a special "All Skids" entry.
        /// Executes initial board search after loading.
        /// </summary>
        [RelayCommand]
        public async Task LoadAsync()
        {
            var types = await _boardService.GetBoardTypesAsync();
            BoardTypes.Clear();
            BoardTypes.Add(string.Empty); // Blank option to show all
            foreach (var t in types) BoardTypes.Add(t);

            var allSkids = await _boardService.ExtractSkidsAsync();
            Skids.Clear();
            Skids.Add(new SkidDto { SkidID = 0, SkidName = "<All Skids>" });
            foreach (var s in allSkids) Skids.Add(s);

            await SearchAsync();
        }

        // ------------------------------
        // Toggle Date Mode
        // ------------------------------

        /// <summary>
        /// Toggles the date filter mode between PrepDate and ShipDate.
        /// Updates dependent display properties.
        /// </summary>
        [RelayCommand]
        public void ToggleDateMode()
        {
            UseShipDate = !UseShipDate;
            OnPropertyChanged(nameof(DateModeLabel));
            OnPropertyChanged(nameof(DateModeButtonText));
        }

        // ------------------------------
        // Export CSV
        // ------------------------------

        /// <summary>
        /// Prompts the user to export current board results to CSV format.
        /// Data is written to the filesystem in a platform-specific directory.
        /// </summary>
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

            var exportFilter = new BoardFilterDto
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
                    _ => (bool?)null // "Both"
                },
                PageNumber = null, // <-- no paging
                PageSize = null
            };

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
        // Search Commands
        // ------------------------------

        /// <summary>
        /// Clears pagination to the first page and initiates a new search.
        /// </summary>
        [RelayCommand]
        public async Task SearchAsync()
        {
            PageNumber = 1;
            await LoadPageAsync(PageNumber);
        }

        /// <summary>
        /// Retrieves a page of board records using the active filters.
        /// Also checks if another page of results exists.
        /// </summary>
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
                ShipDateTo = UseShipDate ? DateTo : null,
                IsShipped = SelectedIsShippedOption switch
                {
                    "Shipped" => true,
                    "Not Shipped" => false,
                    _ => (bool?)null // "Both"
                }

            };

            var results = (await _boardService.GetBoardsAsync(filter)).ToList();

            Boards.Clear();
            foreach (var b in results)
                Boards.Add(b);

            // Preload next page to determine if pagination is available.
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
                ShipDateTo = UseShipDate ? DateTo : null,
                IsShipped = SelectedIsShippedOption switch
                {
                    "Shipped" => true,
                    "Not Shipped" => false,
                    _ => (bool?)null // "Both"
                }

            };

            var nextPageResults = await _boardService.GetBoardsAsync(nextPageFilter);
            HasNextPage = nextPageResults.Any();
        }

        /// <summary>
        /// Moves to the next page if more records exist and loads it.
        /// </summary>
        [RelayCommand]
        private async Task NextPageAsync()
        {
            if (!HasNextPage) return;

            PageNumber++;
            await LoadPageAsync(PageNumber);
        }

        /// <summary>
        /// Moves to the previous page if not already on the first.
        /// </summary>
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

