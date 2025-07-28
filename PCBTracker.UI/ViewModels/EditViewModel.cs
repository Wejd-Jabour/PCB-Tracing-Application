﻿using CommunityToolkit.Mvvm.ComponentModel;
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
    /// ViewModel for the Edit page.
    /// Enables filtering of boards, deleting records by serial number,
    /// applying ship dates to skids, paginating results, and exporting to CSV.
    /// </summary>
    public partial class EditViewModel : ObservableObject
    {
        private readonly IBoardService _boardService;

        // -----------------------------
        // Observable Collections
        // -----------------------------

        [ObservableProperty]
        private ObservableCollection<BoardDto> boards = new(); // Holds board search results.

        [ObservableProperty]
        private ObservableCollection<string> boardTypes = new(); // Board type filter list.

        [ObservableProperty]
        private ObservableCollection<SkidDto> skids = new(); // Skid filter list.

        // -----------------------------
        // Filter Inputs
        // -----------------------------

        [ObservableProperty]
        private string serialNumberFilter = string.Empty; // Text to match SerialNumber (contains).

        [ObservableProperty]
        private string selectedBoardTypeFilter = string.Empty; // Selected BoardType to match.

        [ObservableProperty]
        private SkidDto? selectedSkidFilter = null; // Skid selection; null or SkidID = 0 means "All".

        [ObservableProperty]
        private DateTime dateFrom = DateTime.Today.AddMonths(-1); // Start of date filter range.

        [ObservableProperty]
        private DateTime dateTo = DateTime.Today; // End of date filter range.

        [ObservableProperty]
        private bool useShipDate = false; // Whether to filter on ShipDate or PrepDate.

        [ObservableProperty]
        bool isShipped = false;

        [ObservableProperty]
        private string selectedIsShippedOption = "Both";
        public List<string> IsShippedOptions { get; } = new() { "Both", "Shipped", "Not Shipped" };

        // -----------------------------
        // Edit Operations
        // -----------------------------

        [ObservableProperty]
        private string removeSerialNumber = string.Empty; // Serial number to delete.

        [ObservableProperty]
        private SkidDto? applyShipDateSkid = null; // Skid to which a ship date will be applied.

        [ObservableProperty]
        private DateTime newShipDate = DateTime.Today; // Ship date to apply to the above skid.

        // -----------------------------
        // Pagination
        // -----------------------------

        [ObservableProperty]
        private int pageNumber = 1; // Current page number (1-based index).

        [ObservableProperty]
        private bool hasNextPage; // Whether another page exists after current.

        // -----------------------------
        // Display Strings
        // -----------------------------

        public string DateModeLabel => useShipDate ? "Filtering by Ship Date" : "Filtering by Prep Date";

        public string DateModeButtonText => useShipDate ? "Switch to Prep Date" : "Switch to Ship Date";

        // -----------------------------
        // Constructor
        // -----------------------------

        public EditViewModel(IBoardService boardService)
        {
            _boardService = boardService; // DI-injected service for board data operations.
        }

        // -----------------------------
        // Initialization
        // -----------------------------

        [RelayCommand]
        public async Task LoadAsync()
        {
            // Get board types from service and populate the list.
            var types = await _boardService.GetBoardTypesAsync();
            BoardTypes.Clear();
            BoardTypes.Add(string.Empty); // Add blank option for "All"
            foreach (var t in types)
                BoardTypes.Add(t);

            // Get skids as DTOs for filtering.
            var allSkids = await _boardService.ExtractSkidsAsync();
            Skids.Clear();
            Skids.Add(new SkidDto { SkidID = 0, SkidName = "<All Skids>" }); // Special entry
            foreach (var s in allSkids)
                Skids.Add(s);

            // Load the first page of search results.
            await SearchAsync();
        }

        // -----------------------------
        // Date Mode Toggle
        // -----------------------------

        [RelayCommand]
        public void ToggleDateMode()
        {
            UseShipDate = !UseShipDate; // Flip mode
            OnPropertyChanged(nameof(DateModeLabel)); // Update label
            OnPropertyChanged(nameof(DateModeButtonText)); // Update button text
        }

        // -----------------------------
        // Search
        // -----------------------------

        [RelayCommand]
        public async Task SearchAsync()
        {
            PageNumber = 1;
            await LoadPageAsync(PageNumber); // Start from page 1
        }

        // -----------------------------
        // Record Deletion
        // -----------------------------

        [RelayCommand]
        public async Task RemoveBoardAsync()
        {
            // Skip if no input
            if (string.IsNullOrWhiteSpace(RemoveSerialNumber))
                return;

            // Delete by serial number
            await _boardService.DeleteBoardBySerialAsync(RemoveSerialNumber);

            // Reload results
            await SearchAsync();

            // Confirmation
            await App.Current.MainPage.DisplayAlert("Removed", $"Board {RemoveSerialNumber} deleted.", "OK");
        }

        // -----------------------------
        // Ship Date Application
        // -----------------------------

        [RelayCommand]
        public async Task ApplyShipDateToSkidAsync()
        {
            // Must have valid skid selected (ID != 0)
            if (ApplyShipDateSkid == null || ApplyShipDateSkid.SkidID == 0)
                return;

            await _boardService.UpdateShipDateForSkidAsync(ApplyShipDateSkid.SkidID, NewShipDate);

            await SearchAsync();

            await App.Current.MainPage.DisplayAlert("Updated", $"Applied ship date to Skid {ApplyShipDateSkid.SkidName}.", "OK");
        }

        // -----------------------------
        // CSV Export
        // -----------------------------

        [RelayCommand]
        private async Task ExportCsvAsync()
        {
            bool confirm = await App.Current.MainPage.DisplayAlert(
                "Ready to Export?",
                "Would you like to export this data to CSV?",
                "Yes",
                "No");

            if (!confirm)
                return;

            var sb = new System.Text.StringBuilder();

            // CSV header
            sb.AppendLine("SerialNumber,PartNumber,BoardType,PrepDate,ShipDate,IsShipped,SkidID");

            // Rows
            foreach (var b in Boards)
            {
                var shipDate = b.ShipDate?.ToString("yyyy-MM-dd") ?? "";
                sb.AppendLine($"{b.SerialNumber},{b.PartNumber},{b.BoardType},{b.PrepDate:yyyy-MM-dd},{shipDate},{b.IsShipped},{b.SkidID}");
            }

            // Filename with timestamp
            var fileName = $"EditExport_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

            // Platform-specific folder
            string folderPath;

            #if WINDOWS
                folderPath = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            #else
                folderPath = FileSystem.CacheDirectory;
            #endif
        
            // Final path and write to disk
            var filePath = System.IO.Path.Combine(folderPath, fileName);
            System.IO.File.WriteAllText(filePath, sb.ToString());

            // Success alert
            await App.Current.MainPage.DisplayAlert("Export Complete", $"File saved to:\n{filePath}", "OK");
        }

        // -----------------------------
        // Pagination
        // -----------------------------

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

        /// <summary>
        /// Loads a specific page of board data using current filters.
        /// Also preloads the next page to determine if paging forward is possible.
        /// </summary>
        private async Task LoadPageAsync(int page)
        {
            // Build active filter
            var filter = new BoardFilterDto
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
                PageNumber = page,
                PageSize = 50
            };

            // Fetch current page
            var results = (await _boardService.GetBoardsAsync(filter)).ToList();

            Boards.Clear();
            foreach (var b in results)
                Boards.Add(b);

            // Build next-page filter
            var nextFilter = new BoardFilterDto
            {
                SerialNumber = filter.SerialNumber,
                BoardType = filter.BoardType,
                SkidId = filter.SkidId,
                PrepDateFrom = filter.PrepDateFrom,
                PrepDateTo = filter.PrepDateTo,
                ShipDateFrom = filter.ShipDateFrom,
                ShipDateTo = filter.ShipDateTo,
                IsShipped = SelectedIsShippedOption switch
                {
                    "Shipped" => true,
                    "Not Shipped" => false,
                    _ => (bool?)null // "Both"
                },
                PageNumber = page + 1,
                PageSize = filter.PageSize
            };

            var nextPageResults = await _boardService.GetBoardsAsync(nextFilter);
            HasNextPage = nextPageResults.Any(); // Flag next page availability
        }
    }
}
