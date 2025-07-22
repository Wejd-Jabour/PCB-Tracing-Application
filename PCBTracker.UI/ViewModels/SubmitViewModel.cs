using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PCBTracker.Domain.DTOs;
using PCBTracker.Domain.Entities;
using PCBTracker.Services.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace PCBTracker.UI.ViewModels
{
    /// <summary>
    /// ViewModel for SubmitPage. Manages form state, lookup loading, validation,
    /// and submission of new boards via the IBoardService.
    /// </summary>
    public partial class SubmitViewModel : ObservableObject
    {
        private readonly IBoardService _boardService;

        /// <summary>
        /// IBoardService is injected to handle business logic and EF Core operations.
        /// </summary>
        public SubmitViewModel(IBoardService boardService)
        {
            _boardService = boardService;
            PrepDate = DateTime.Today;  // Default PrepDate to today
        }

        // Collections bound to Picker controls in XAML:
        [ObservableProperty]
        private ObservableCollection<string> boardTypes = new();

        [ObservableProperty]
        private ObservableCollection<Skid> skids = new();

        // Form fields bound to various Entry/Picker/DatePicker/Switch controls:
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
        private string serialNumber = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
        private string partNumber = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
        private string selectedBoardType;

        [ObservableProperty]
        private DateTime prepDate;

        [ObservableProperty]
        private bool isShipped;

        [ObservableProperty]
        private DateTime? shipDate;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
        private Skid selectedSkid;

        /// <summary>
        /// Loads board types and skids each time the page appears.
        /// Ensures at least one Skid exists and selects the latest one.
        /// </summary>
        [RelayCommand]
        public async Task LoadAsync()
        {
            // Fetch and populate board types for the Picker
            var types = await _boardService.GetBoardTypesAsync();
            BoardTypes.Clear();
            foreach (var t in types)
                BoardTypes.Add(t);

            // Fetch existing skids or create one if none exist
            var existing = await _boardService.GetSkidsAsync();
            Skids.Clear();
            foreach (var s in existing)
                Skids.Add(s);

            if (Skids.Count == 0)
            {
                var newSkid = await _boardService.CreateNewSkidAsync();
                Skids.Add(newSkid);
            }

            // Default to the most recently created skid
            SelectedSkid = Skids[^1];

            // If already marked shipped, ensure ShipDate is initialized
            if (IsShipped && ShipDate == null)
                ShipDate = PrepDate;
        }

        /// <summary>
        /// Enables the Submit button only when required fields are filled.
        /// </summary>
        private bool CanSubmit()
            => !string.IsNullOrWhiteSpace(SerialNumber)
               && !string.IsNullOrWhiteSpace(PartNumber)
               && !string.IsNullOrWhiteSpace(SelectedBoardType)
               && SelectedSkid != null;

        /// <summary>
        /// Gathers form data into a BoardDto and sends it to the service.
        /// Shows a success alert and clears the serial field for the next scan.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanSubmit))]
        private async Task SubmitAsync()
        {
            var dto = new BoardDto
            {
                SerialNumber = SerialNumber,
                PartNumber = PartNumber,
                BoardType = SelectedBoardType,
                PrepDate = PrepDate,
                IsShipped = IsShipped,
                ShipDate = IsShipped ? ShipDate : null,
                SkidID = SelectedSkid.SkidID
            };

            await _boardService.CreateBoardAsync(dto);

            await App.Current.MainPage.DisplayAlert("Success", "Board submitted.", "OK");

            // Clear only the serial number for the next scan:
            SerialNumber = string.Empty;
        }

        /// <summary>
        /// Cycle to the next Skid by creating a new one and selecting it.
        /// Bound to the “Cycle Skid” button in the UI.
        /// </summary>
        [RelayCommand]
        private async Task ChangeSkidAsync()
        {
            // Create a brand-new skid in the DB
            var newSkid = await _boardService.CreateNewSkidAsync();

            // Add it to the collection so the Picker updates...
            Skids.Add(newSkid);

            // ...and select it immediately
            SelectedSkid = newSkid;
        }

    }
}