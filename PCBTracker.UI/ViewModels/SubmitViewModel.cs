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

        private static readonly IReadOnlyDictionary<string, string> _partNumberMap =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["LE"] = "ASY-G8GMLESBH-P-ATLR07MR1",
                ["LE Upgrade"] = "Unknown",
                ["SAD"] = "ASY-G8GMSADSBH-P-ATLR03MR1",
                ["SAD Upgrade"] = "ASY-GSGMSADB-UG-KIT-P-ATLR05MR2",
                ["SAT"] = "ASY-G8GMSATSBH-P-ATLR02MR1",
                ["SAT Upgrade"] = "ASY-G8GMSATB-UG-KIT-P-ATLR03MR1",
            };

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
        partial void OnSerialNumberChanged(string oldValue, string newValue)
            => DebounceAutoSubmit();

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
               && SelectedSkid != null
            && (SelectedSkid.designatedType == null || SelectedSkid.designatedType == SelectedBoardType);

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

            await _boardService.CreateBoardAndClaimSkidAsync(dto);

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


        private CancellationTokenSource _autoSubmitCts;

        private void DebounceAutoSubmit()
        {
            // cancel any pending run
            _autoSubmitCts?.Cancel();
            _autoSubmitCts = new CancellationTokenSource();
            var token = _autoSubmitCts.Token;

            // fire-and-forget background task
            _ = Task.Run(async () =>
            {
                try
                {
                    // wait 1 second (1000 ms)
                    await Task.Delay(1000, token);

                    // if not cancelled and can submit, trigger the command on the UI thread
                    if (!token.IsCancellationRequested && CanSubmit())
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                            SubmitCommand.Execute(null)
                        );
                    }
                }
                catch (TaskCanceledException) { /* noop */ }
            });
        }

        partial void OnSelectedBoardTypeChanged(string oldValue, string newValue)
        {
            if (!string.IsNullOrWhiteSpace(newValue)
                && _partNumberMap.TryGetValue(newValue, out var pn))
            {
                PartNumber = pn;
            }
            else
            {
                PartNumber = string.Empty;
            }
        }

    }
}