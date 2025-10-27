﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.ApplicationModel; // MainThread
using PCBTracker.Domain.DTOs;
using PCBTracker.Domain.Entities;
using PCBTracker.Services.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Maui.Controls; // for Navigation to modal
using PCBTracker.UI.Views;     // ConfirmSkidPage

namespace PCBTracker.UI.ViewModels
{
    /// <summary>
    /// ViewModel for SubmitPage.
    /// - Add-new-type flow (sentinel + Entry)
    /// - PN auto-fill from latest board for existing types (fallback to static map)
    /// - Per-type serial-length enforcement (matches earliest recorded for that type)
    /// - Skid designation enforcement
    /// - NEW: Confirm Skid modal + sequential new skid
    /// - NEW: Old-skid password lock (session-scoped unlock per skid)
    /// </summary>
    public partial class SubmitViewModel : ObservableObject
    {
        private readonly IBoardService _boardService;

        // Fallback PN map (only when DB has no prior PN for the type)
        private static readonly IReadOnlyDictionary<string, string> _partNumberMap =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["LE"] = "ASY-G8GMLESBH-P-ATLR07MR1",
                ["LE Upgrade"] = "ASY-G8GMLEB-UG-KIT-P-ATLR05MR1",
                ["LE Tray"] = "ASY-G8GMLESB-P-ATLR06MR0",
                ["SAD"] = "ASY-G8GMSADSBH-P-ATLR03MR1",
                ["SAD Upgrade"] = "ASY-GSGMSADB-UG-KIT-P-ATLR05MR2",
                ["SAT"] = "ASY-G8GMSATSBH-P-ATLR02MR1",
                ["SAT Upgrade"] = "ASY-G8GMSATB-UG-KIT-P-ATLR03MR1",
            };

        private const string NewTypeSentinel = "(Add new type…)";

        public SubmitViewModel(IBoardService boardService)
        {
            _boardService = boardService;
            PrepDate = DateTime.Today;
        }

        // ------------------------------
        // Bindable Properties
        // ------------------------------

        [ObservableProperty] private ObservableCollection<string> boardTypes = new();
        [ObservableProperty] private ObservableCollection<Skid> skids = new();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
        private string serialNumber = string.Empty;

        partial void OnSerialNumberChanged(string oldValue, string newValue) => DebounceAutoSubmit();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
        private string partNumber = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
        private string selectedBoardType;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
        private bool isCreatingNewType;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
        private string newBoardTypeText = string.Empty;

        [ObservableProperty] private DateTime prepDate;
        [ObservableProperty] private bool isShipped;
        [ObservableProperty] private DateTime? shipDate;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
        [NotifyCanExecuteChangedFor(nameof(PageBackwardCommand))]
        [NotifyCanExecuteChangedFor(nameof(PageForwardCommand))]
        private Skid selectedSkid;

        [ObservableProperty] private int skidBoardCount;
        public string SkidBoardCountLabel => $"Boards on this Skid: {SkidBoardCount:N0}";

        public string CurrentSkidType => SelectedSkid?.designatedType ?? "(unassigned)";

        // === NEW: Lock/Unlock state ===
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
        private bool isSkidLocked; // true means scanning/submit disabled

        private readonly HashSet<int> _unlockedSkids = new();

        // ------------------------------
        // Load (types + skids)
        // ------------------------------

        [RelayCommand]
        public async Task LoadAsync()
        {
            // Types
            var types = await _boardService.GetBoardTypesAsync();
            BoardTypes.Clear();
            BoardTypes.Add(NewTypeSentinel);
            foreach (var t in types) BoardTypes.Add(t);

            // Skids
            var recent = await _boardService.GetRecentSkidsAsync(100);
            Skids.Clear();
            foreach (var s in recent) Skids.Add(s);

            if (Skids.Count == 0)
            {
                var ns = await _boardService.CreateNewSkidAsync();
                Skids.Add(ns);
            }

            SelectedSkid = Skids[^1];
            await UpdateLockForSelectedSkidAsync();
            OnPropertyChanged(nameof(CurrentSkidType));
            await RefreshSkidCountAsync();
        }

        // ------------------------------
        // Skid nav + Confirm Skid flow
        // ------------------------------

        [RelayCommand(CanExecute = nameof(CanPageBackward))]
        private async Task PageBackwardAsync()
        {
            if (Skids.Count == 0 || SelectedSkid == null) return;
            var idx = Skids.IndexOf(SelectedSkid);
            if (idx > 0)
            {
                SelectedSkid = Skids[idx - 1];
                await UpdateLockForSelectedSkidAsync();
                OnPropertyChanged(nameof(CurrentSkidType));
                await RefreshSkidCountAsync();
            }
        }
        private bool CanPageBackward() => SelectedSkid != null && Skids.IndexOf(SelectedSkid) > 0;

        [RelayCommand(CanExecute = nameof(CanPageForward))]
        private async Task PageForwardAsync()
        {
            if (Skids.Count == 0 || SelectedSkid == null) return;
            var idx = Skids.IndexOf(SelectedSkid);
            if (idx < Skids.Count - 1)
            {
                SelectedSkid = Skids[idx + 1];
                await UpdateLockForSelectedSkidAsync();
                OnPropertyChanged(nameof(CurrentSkidType));
                await RefreshSkidCountAsync();
            }
        }
        private bool CanPageForward() => SelectedSkid != null && Skids.IndexOf(SelectedSkid) < Skids.Count - 1;

        // (Old "Start New Skid" kept for any other call sites; not used by the renamed button.)
        [RelayCommand]
        private async Task ChangeSkidAsync()
        {
            var newSkid = await _boardService.CreateNewSkidAsync();
            if (newSkid != null)
            {
                Skids.Add(newSkid);
                SelectedSkid = newSkid;
                await UpdateLockForSelectedSkidAsync();
                OnPropertyChanged(nameof(CurrentSkidType));
                await RefreshSkidCountAsync();
            }
        }

        // === NEW: Confirm Skid command (used by renamed button) ===
        [RelayCommand]
        private async Task OpenConfirmSkidAsync()
        {
            if (SelectedSkid == null)
            {
                await App.Current.MainPage.DisplayAlert("No Skid", "Select a skid first.", "OK");
                return;
            }

            // Load boards for the review modal
            var boards = (await _boardService.GetBoardsBySkidAsync(SelectedSkid.SkidID)).ToList();

            var modal = new ConfirmSkidPage(SelectedSkid, boards);
            await App.Current.MainPage.Navigation.PushModalAsync(modal);
            var confirmed = await modal.WaitForResultAsync();
            await App.Current.MainPage.Navigation.PopModalAsync();

            if (!confirmed) return;

            // Re-lock the skid we just confirmed
            var justConfirmedId = SelectedSkid.SkidID;
            _unlockedSkids.Remove(justConfirmedId);

            // Decide what to do next based on "latestness"
            var maxId = await _boardService.GetMaxSkidIdAsync();
            var isCurrentLatest = (justConfirmedId == maxId);

            if (isCurrentLatest)
            {
                // We were on the latest skid → create the next sequential skid
                var newSkid = await _boardService.CreateNewSkidAsync();
                Skids.Add(newSkid);

                SelectedSkid = newSkid;
                _unlockedSkids.Add(newSkid.SkidID);   // latest is unlocked
                IsSkidLocked = false;
            }
            else
            {
                // We were on an older skid → DO NOT create a new one.
                // Jump to the most recent skid instead.
                await SelectMostRecentSkidAsync();
            }

            // Clear inputs (keep type selection)
            SerialNumber = string.Empty;
            PartNumber = string.Empty;
            IsShipped = false;
            ShipDate = null;

            OnPropertyChanged(nameof(CurrentSkidType));
            await RefreshSkidCountAsync();
        }


        // When skid changes (via picker), update labels/counts and lock state
        partial void OnSelectedSkidChanged(Skid oldValue, Skid newValue)
        {
            _ = UpdateLockForSelectedSkidAsync();
            OnPropertyChanged(nameof(CurrentSkidType));
            _ = RefreshSkidCountAsync();
        }

        private async Task UpdateLockForSelectedSkidAsync()
        {
            if (SelectedSkid == null) { IsSkidLocked = true; return; }

            var maxId = await _boardService.GetMaxSkidIdAsync();
            var isLatest = SelectedSkid.SkidID == maxId;

            if (isLatest)
            {
                IsSkidLocked = false;
                _unlockedSkids.Add(SelectedSkid.SkidID);
            }
            else
            {
                IsSkidLocked = !_unlockedSkids.Contains(SelectedSkid.SkidID);
            }
        }

        // Manual unlock command (visible only when locked)
        [RelayCommand]
        private async Task UnlockOldSkidAsync()
        {
            if (SelectedSkid == null) return;

            if (string.IsNullOrWhiteSpace(App.LoggedInPassword))
            {
                await App.Current.MainPage.DisplayAlert(
                    "Cannot Unlock",
                    "No login password is available in this session. Please log out and log back in, or switch to role-based unlock.",
                    "OK");
                return;
            }

            var entered = await App.Current.MainPage.DisplayPromptAsync(
                "Unlock Skid",
                $"Enter the login password to unlock Skid {SelectedSkid.SkidName} for this session:",
                "Unlock", "Cancel", placeholder: "Password", maxLength: 128, keyboard: Keyboard.Text);

            if (entered == null) return; // cancelled

            if (string.Equals(entered.Trim(), App.LoggedInPassword.Trim(), StringComparison.Ordinal))
            {
                _unlockedSkids.Add(SelectedSkid.SkidID);
                IsSkidLocked = false;
                await App.Current.MainPage.DisplayAlert("Unlocked", "You can now scan/submit to this skid.", "OK");
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Denied", "Incorrect password.", "OK");
            }
        }


        private async Task SelectMostRecentSkidAsync()
        {
            var maxId = await _boardService.GetMaxSkidIdAsync();

            // Ensure Skids list has the latest
            if (!Skids.Any(s => s.SkidID == maxId))
            {
                var all = await _boardService.GetRecentSkidsAsync(100);
                Skids.Clear();
                foreach (var s in all) Skids.Add(s);
            }

            var latest = Skids.OrderBy(s => s.SkidID).LastOrDefault();
            if (latest != null)
            {
                SelectedSkid = latest;                // triggers UpdateLockForSelectedSkidAsync
                _unlockedSkids.Add(latest.SkidID);    // ensure unlocked
                IsSkidLocked = false;
            }
        }


        // ------------------------------
        // Validation & Submit
        // ------------------------------

        private bool CanSubmit()
        {
            if (IsSkidLocked) return false;

            var effectiveType = IsCreatingNewType ? NewBoardTypeText?.Trim() : SelectedBoardType;
            return !string.IsNullOrWhiteSpace(SerialNumber)
                && !string.IsNullOrWhiteSpace(PartNumber)
                && !string.IsNullOrWhiteSpace(effectiveType)
                && SelectedSkid != null
                && (SelectedSkid!.designatedType == null
                    || string.Equals(SelectedSkid.designatedType, effectiveType, StringComparison.OrdinalIgnoreCase));
        }

        [RelayCommand(CanExecute = nameof(CanSubmit))]
        private async Task SubmitAsync()
        {
            if (SelectedSkid == null) return;

            var effectiveType = IsCreatingNewType ? NewBoardTypeText?.Trim() : SelectedBoardType;

            // Enforce skid designation (if any)
            if (!string.IsNullOrEmpty(SelectedSkid.designatedType)
                && !string.Equals(SelectedSkid.designatedType, effectiveType, StringComparison.OrdinalIgnoreCase))
            {
                await App.Current.MainPage.DisplayAlert(
                    "Error",
                    $"This skid is for {SelectedSkid.designatedType}. Select a matching type or start a new skid.",
                    "OK");
                return;
            }

            // Serial-length rule: match earliest record for that type (if any)
            var existingForType = await _boardService.GetBoardsAsync(new BoardFilterDto
            {
                BoardType = effectiveType,
                PageNumber = null,
                PageSize = null
            });

            int? canonicalLen = null;
            var list = existingForType?.ToList() ?? new();
            if (list.Count > 0)
            {
                var earliest = list.LastOrDefault(b => !string.IsNullOrWhiteSpace(b.SerialNumber)); // service returns CreatedAt DESC
                if (earliest != null) canonicalLen = earliest.SerialNumber.Length;
            }

            if (canonicalLen.HasValue && SerialNumber.Length != canonicalLen.Value)
            {
                await App.Current.MainPage.DisplayAlert(
                    "Invalid Serial Length",
                    $"For board type '{effectiveType}', serials must be {canonicalLen.Value} characters.",
                    "OK");
                return;
            }

            var dto = new BoardDto
            {
                SerialNumber = SerialNumber?.Trim(),
                PartNumber = PartNumber?.Trim(),
                BoardType = effectiveType,
                PrepDate = PrepDate,
                IsShipped = IsShipped,
                ShipDate = IsShipped ? ShipDate ?? PrepDate : null,
                SkidID = SelectedSkid.SkidID
            };

            try
            {
                await _boardService.CreateBoardAndClaimSkidAsync(dto);

                // Refresh skid designation (in case it got set server-side)
                var updatedSkids = await _boardService.GetSkidsAsync();
                var refreshed = updatedSkids.FirstOrDefault(s => s.SkidID == SelectedSkid.SkidID);
                if (refreshed != null)
                {
                    SelectedSkid.designatedType = refreshed.designatedType;
                    OnPropertyChanged(nameof(CurrentSkidType));
                }

                // Clear only SerialNumber (keep selections for speed)
                SerialNumber = string.Empty;

                await RefreshSkidCountAsync();
            }
            catch (DbUpdateException dbEx) when (dbEx.InnerException is SqlException sqlEx && sqlEx.Number == 2627)
            {
                await App.Current.MainPage.DisplayAlert("Serial Number Taken",
                    "That serial number already exists. Each board must be unique.",
                    "OK");
                SerialNumber = string.Empty;
            }
            catch (Exception ex)
            {
                var msg = string.IsNullOrWhiteSpace(ex.Message)
                    ? "An unexpected error occurred while saving. Please try again."
                    : ex.Message;
                await App.Current.MainPage.DisplayAlert("Error", msg, "OK");
            }
        }

        // ------------------------------
        // Debounced Auto-Submit
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
                    await Task.Delay(2700, token);
                    if (!token.IsCancellationRequested && CanSubmit())
                    {
                        MainThread.BeginInvokeOnMainThread(() => SubmitCommand.Execute(null));
                    }
                }
                catch (TaskCanceledException) { }
            });
        }

        /// <summary>
        /// Type changed:
        /// • If sentinel → keep in new-type mode and clear PN.
        /// • Else → load latest PN from DB; fallback to static map.
        /// </summary>
        partial void OnSelectedBoardTypeChanged(string oldValue, string newValue)
        {
            IsCreatingNewType = string.Equals(newValue, NewTypeSentinel, StringComparison.Ordinal);
            if (IsCreatingNewType) { PartNumber = string.Empty; return; }

            if (!string.IsNullOrWhiteSpace(newValue))
                _ = LoadLatestPartNumberForTypeAsync(newValue);
            else
                PartNumber = string.Empty;
        }

        private async Task LoadLatestPartNumberForTypeAsync(string boardType)
        {
            try
            {
                var boards = await _boardService.GetBoardsAsync(new BoardFilterDto
                {
                    BoardType = boardType,
                    PageNumber = null,
                    PageSize = null
                });

                var latestPn = boards
                    .OrderByDescending(b => b.ShipDate ?? b.PrepDate)
                    .Select(b => b.PartNumber)
                    .FirstOrDefault(pn => !string.IsNullOrWhiteSpace(pn));

                if (string.IsNullOrWhiteSpace(latestPn)
                    && _partNumberMap.TryGetValue(boardType, out var mapped))
                {
                    latestPn = mapped;
                }

                MainThread.BeginInvokeOnMainThread(() => PartNumber = latestPn ?? string.Empty);
            }
            catch
            {
                var fallback = _partNumberMap.TryGetValue(boardType, out var mapped) ? mapped : string.Empty;
                MainThread.BeginInvokeOnMainThread(() => PartNumber = fallback);
            }
        }

        // ------------------------------
        // Helpers
        // ------------------------------

        private async Task RefreshSkidCountAsync()
        {
            if (SelectedSkid == null)
            {
                SkidBoardCount = 0;
                OnPropertyChanged(nameof(SkidBoardCountLabel));
                return;
            }

            var all = await _boardService.GetBoardsAsync(new BoardFilterDto
            {
                SkidId = SelectedSkid.SkidID,
                PageNumber = null,
                PageSize = null
            });

            SkidBoardCount = all.Count();
            OnPropertyChanged(nameof(SkidBoardCountLabel));
        }
    }
}
