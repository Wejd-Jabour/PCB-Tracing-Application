using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PCBTracker.Domain.DTOs;
using PCBTracker.Domain.Entities;
using PCBTracker.Services.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace PCBTracker.UI.ViewModels
{
    /// <summary>
    /// ViewModel for SubmitPage.
    /// Manages form state, validation, list loading, and board submission logic.
    /// Implements INotifyPropertyChanged via ObservableObject.
    /// </summary>
    public partial class SubmitViewModel : ObservableObject
    {
        private readonly IBoardService _boardService;

        /// <summary>
        /// Static map between board types and their corresponding part numbers.
        /// Used to auto-fill the PartNumber field when a board type is selected.
        /// </summary>
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
        /// Constructor receives IBoardService dependency for data operations.
        /// Initializes default prep date to today.
        /// </summary>
        public SubmitViewModel(IBoardService boardService)
        {
            _boardService = boardService;
            PrepDate = DateTime.Today;
        }

        // ------------------------------
        // Bindable Properties
        // ------------------------------

        [ObservableProperty]
        private ObservableCollection<string> boardTypes = new();

        [ObservableProperty]
        private ObservableCollection<Skid> skids = new();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
        private string serialNumber = string.Empty;

        // Automatically triggers auto-submit debounce on value change
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
        [NotifyCanExecuteChangedFor(nameof(PageBackwardCommand))]
        [NotifyCanExecuteChangedFor(nameof(PageForwardCommand))]
        private Skid selectedSkid;

        // ------------------------------
        // Lifecycle Command
        // ------------------------------

        /// <summary>
        /// Loads list values on page entry.
        /// Populates board types and skids; creates skid if none exist.
        /// </summary>
        [RelayCommand]
        public async Task LoadAsync()
        {
            var types = await _boardService.GetBoardTypesAsync();
            BoardTypes.Clear();
            foreach (var t in types)
                BoardTypes.Add(t);

            var recent = await _boardService.GetRecentSkidsAsync(10);
            Skids.Clear();
            foreach (var s in recent)
                Skids.Add(s);

            SelectedSkid = Skids.LastOrDefault();

            if (Skids.Count == 0)
            {
                var newSkid = await _boardService.CreateNewSkidAsync();
                Skids.Add(newSkid);
            }

            SelectedSkid = Skids[^1];

            if (IsShipped && ShipDate == null)
                ShipDate = PrepDate;
        }

        // ------------------------------
        // Submit Validation
        // ------------------------------

        /// <summary>
        /// Determines whether the Submit button is enabled.
        /// All required fields must be filled and the selected skid must match the type (if constrained).
        /// </summary>
        private bool CanSubmit()
            => !string.IsNullOrWhiteSpace(SerialNumber)
            && !string.IsNullOrWhiteSpace(PartNumber)
            && !string.IsNullOrWhiteSpace(SelectedBoardType)
            && SelectedSkid != null
            && (SelectedSkid.designatedType == null || SelectedSkid.designatedType == SelectedBoardType);

        // ------------------------------
        // Submit Command
        // ------------------------------

        /// <summary>
        /// Constructs a BoardDto and submits it to the service.
        /// Handles validation errors, uniqueness conflicts, and shows result dialogs.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanSubmit))]
        private async Task SubmitAsync()
        {
            try
            {
                if (SerialNumber.Length != 16)
                    throw new Exception("Invalid Serial Number Length");

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

                //await App.Current.MainPage.DisplayAlert("Success", "Board submitted.", "OK");

                SerialNumber = string.Empty;

                OnPropertyChanged(nameof(CurrentSkidType));
            }
            catch (DbUpdateException dbEx) when (dbEx.InnerException is SqlException sqlEx
                                                 && sqlEx.Message.Contains("cannot be tracked"))
            {
                await App.Current.MainPage.DisplayAlert(
                    "Duplicate Board",
                    "A board with that serial number already exists in this session. Please check your Serial Number and try again, or edit the existing record.",
                    "OK");
            }
            catch (DbUpdateException dbEx) when (dbEx.InnerException is SqlException sqlEx
                                                 && sqlEx.Number == 2627)
            {
                await App.Current.MainPage.DisplayAlert(
                    "Serial Number Taken",
                    "That serial number is already in use. Each board must have a unique Serial Number.",
                    "OK");
            }
            catch (Exception ex)
            {
                string message = !string.IsNullOrWhiteSpace(ex.Message)
                    ? ex.Message
                    : "An unexpected error occurred while saving. Please try again or contact support if it persists.";

                await App.Current.MainPage.DisplayAlert(
                    "Error",
                    message,
                    "OK");
            }
        }

        // ------------------------------
        // Change Skid Command
        // ------------------------------

        /// <summary>
        /// Creates a new skid and sets it as the selected one.
        /// </summary>
        [RelayCommand]
        private async Task ChangeSkidAsync()
        {
            var newSkid = await _boardService.CreateNewSkidAsync();
            Skids.Add(newSkid);
            SelectedSkid = newSkid;
        }

        // ------------------------------
        // Debounced Auto Submit
        // ------------------------------

        private CancellationTokenSource _autoSubmitCts;

        private void DebounceAutoSubmit()
        {
            _autoSubmitCts?.Cancel();
            _autoSubmitCts = new CancellationTokenSource();
            var token = _autoSubmitCts.Token;

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(1000, token);
                    if (!token.IsCancellationRequested && CanSubmit())
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                            SubmitCommand.Execute(null));
                    }
                }
                catch (TaskCanceledException) { }
            });
        }

        /// <summary>
        /// When board type changes, automatically set PartNumber from lookup table.
        /// </summary>
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

        // ------------------------------
        // Skid Paging Commands
        // ------------------------------

        [RelayCommand(CanExecute = nameof(CanPageBackward))]
        public void PageBackward()
        {
            var idx = Skids.IndexOf(SelectedSkid);
            if (idx > 0)
                SelectedSkid = Skids[idx - 1];
        }

        private bool CanPageBackward()
            => SelectedSkid != null && Skids.IndexOf(SelectedSkid) > 0;

        [RelayCommand(CanExecute = nameof(CanPageForward))]
        public void PageForward()
        {
            var idx = Skids.IndexOf(SelectedSkid);
            if (idx < Skids.Count - 1)
                SelectedSkid = Skids[idx + 1];
        }

        private bool CanPageForward()
            => SelectedSkid != null && Skids.IndexOf(SelectedSkid) < Skids.Count - 1;

        // ------------------------------
        // Skid Type Display
        // ------------------------------

        /// <summary>
        /// Returns the designated type of the currently selected skid or a fallback if unassigned.
        /// </summary>
        public string CurrentSkidType
            => SelectedSkid?.designatedType ?? "(unassigned)";

        /// <summary>
        /// Updates the display when the selected skid changes.
        /// </summary>
        partial void OnSelectedSkidChanged(Skid oldValue, Skid newValue)
        {
            OnPropertyChanged(nameof(CurrentSkidType));
        }
    }
}
