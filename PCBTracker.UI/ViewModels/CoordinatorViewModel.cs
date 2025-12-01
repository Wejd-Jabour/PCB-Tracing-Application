using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PCBTracker.Domain.DTOs;
using PCBTracker.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace PCBTracker.UI.ViewModels
{
    /// <summary>
    /// ViewModel for the Coordinator page.
    /// - Loads recent MaraHolly order lines from the local DB (last 30 days).
    /// - Can trigger a sync from Acumatica via OData.
    /// - Lets the user filter/search and change ProcessingStatus for lines.
    /// </summary>
    public partial class CoordinatorViewModel : ObservableObject
    {
        private readonly IMaraHollyOrderService _orderService;
        private readonly IMaraHollyOrderSyncService _syncService;

        // Backing store for all orders; Orders is the filtered view
        private List<MaraHollyOrderLineDto> _allOrders = new();

        // Statuses that should be hidden unless explicitly filtered
        private static readonly string[] TerminalStatuses = { "Complete", "Void" };

        public CoordinatorViewModel(
            IMaraHollyOrderService orderService,
            IMaraHollyOrderSyncService syncService)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _syncService = syncService ?? throw new ArgumentNullException(nameof(syncService));

            // Filter dropdown (includes "All", "Complete", "Void")
            ProcessingStatusOptions = new ObservableCollection<string>(
                new[] { "All", "Unassigned", "Active", "OnHold", "Cancelled", "Complete", "Void" });

            // Per-row dropdown (no "All", but includes "Complete" + "Void")
            RowProcessingStatusOptions = new ObservableCollection<string>(
                new[] { "Unassigned", "Active", "OnHold", "Cancelled", "Complete", "Void" });

            SelectedProcessingStatusFilter = ProcessingStatusOptions.FirstOrDefault();
        }

        // -----------------------------
        // Collections / state
        // -----------------------------

        [ObservableProperty]
        private ObservableCollection<MaraHollyOrderLineDto> orders = new();

        [ObservableProperty]
        private ObservableCollection<string> processingStatusOptions;

        /// <summary>
        /// Options for the per-row ProcessingStatus picker.
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<string> rowProcessingStatusOptions;

        [ObservableProperty]
        private string? selectedProcessingStatusFilter;

        [ObservableProperty]
        private string? searchText;

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private bool isRefreshing;

        [ObservableProperty]
        private DateTime? lastSyncTime;

        [ObservableProperty]
        private string? errorMessage;

        /// <summary>
        /// Whether there are any orders after filtering.
        /// Useful for showing "no data" messages in the UI.
        /// </summary>
        public bool HasOrders => Orders.Any();

        /// <summary>
        /// Human-readable last sync label for display.
        /// </summary>
        public string LastSyncLabel =>
            LastSyncTime.HasValue
                ? $"Last sync: {LastSyncTime.Value.ToLocalTime():yyyy-MM-dd HH:mm}"
                : "Last sync: never";

        // ---------------------------------
        // Lifecycle: called from the Page
        // ---------------------------------

        /// <summary>
        /// Called by CoordinatorPage.OnAppearing().
        /// Loads recent orders from the local DB and applies filters.
        /// Does NOT hit Acumatica; use RefreshCommand for that.
        /// </summary>
        public async Task LoadAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                ErrorMessage = null;

                await LoadOrdersAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ---------------------------------
        // Commands
        // ---------------------------------

        /// <summary>
        /// Pull-to-refresh / manual sync:
        /// - Calls Acumatica via OData.
        /// - Upserts local DB via sync service.
        /// - Reloads the local orders and updates LastSyncTime.
        /// </summary>
        [RelayCommand]
        private async Task RefreshAsync()
        {
            if (IsRefreshing)
                return;

            try
            {
                IsRefreshing = true;
                ErrorMessage = null;

                // 1. Sync with Acumatica
                await _syncService.SyncAsync();

                // 2. Record when we synced
                LastSyncTime = DateTime.UtcNow;

                // 3. Reload local orders from DB
                await LoadOrdersAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        // Status change commands (still available if you ever wire up swipe actions)

        [RelayCommand]
        private async Task SetActiveAsync(MaraHollyOrderLineDto? order)
        {
            await SetProcessingStatusInternalAsync(order, "Active");
        }

        [RelayCommand]
        private async Task SetOnHoldAsync(MaraHollyOrderLineDto? order)
        {
            await SetProcessingStatusInternalAsync(order, "OnHold");
        }

        [RelayCommand]
        private async Task SetCancelledAsync(MaraHollyOrderLineDto? order)
        {
            await SetProcessingStatusInternalAsync(order, "Cancelled");
        }

        /// <summary>
        /// Confirm button command from each row.
        /// Uses whatever ProcessingStatus the coordinator picked in the row's Picker.
        /// </summary>
        [RelayCommand]
        private async Task ConfirmProcessingStatusAsync(MaraHollyOrderLineDto? order)
        {
            if (order == null)
                return;

            var newStatus = order.ProcessingStatus;

            if (string.IsNullOrWhiteSpace(newStatus))
                return;

            await SetProcessingStatusInternalAsync(order, newStatus);
        }

        // ---------------------------------
        // Internal helpers
        // ---------------------------------

        private async Task LoadOrdersAsync()
        {
            // Fetch recent orders from local DB (last 30 days)
            var list = await _orderService.GetRecentOrdersAsync(days: 30);
            _allOrders = list.ToList();

            ApplyFilters();
        }

        private void ApplyFilters()
        {
            IEnumerable<MaraHollyOrderLineDto> query = _allOrders;

            // Text search: OrderNbr, InventoryId, CustomerOrderNbr
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var text = SearchText.Trim();

                query = query.Where(o =>
                    (!string.IsNullOrEmpty(o.OrderNbr) &&
                     o.OrderNbr.Contains(text, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(o.InventoryId) &&
                     o.InventoryId.Contains(text, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(o.CustomerOrderNbr) &&
                     o.CustomerOrderNbr.Contains(text, StringComparison.OrdinalIgnoreCase)));
            }

            // ProcessingStatus filter rules:
            // - "Complete" => show only completed orders
            // - "Void"     => show only void orders
            // - any other specific status => show that status, excluding Complete/Void
            // - "All" or null/empty       => show everything EXCEPT Complete/Void
            var filter = SelectedProcessingStatusFilter;

            if (string.Equals(filter, "Complete", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(filter, "Void", StringComparison.OrdinalIgnoreCase))
            {
                // Only that terminal status
                query = query.Where(o =>
                    string.Equals(o.ProcessingStatus, filter, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                // Hide terminal statuses by default
                query = query.Where(o =>
                    !TerminalStatuses.Any(ts =>
                        string.Equals(o.ProcessingStatus, ts, StringComparison.OrdinalIgnoreCase)));

                // Apply any other specific filter (Unassigned, Active, OnHold, Cancelled)
                if (!string.IsNullOrWhiteSpace(filter) &&
                    !string.Equals(filter, "All", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(o =>
                        string.Equals(o.ProcessingStatus, filter, StringComparison.OrdinalIgnoreCase));
                }
            }

            // Order by request date, then OrderNbr, then LineNbr
            var finalList = query
                .OrderByDescending(o => o.RequestDate)
                .ThenBy(o => o.OrderNbr)
                .ThenBy(o => o.LineNbr)
                .ToList();

            Orders.Clear();
            foreach (var item in finalList)
            {
                Orders.Add(item);
            }

            OnPropertyChanged(nameof(HasOrders));
        }

        /// <summary>
        /// Central place where we update processing status and then
        /// reload the table so the UI immediately reflects the change.
        /// </summary>
        private async Task SetProcessingStatusInternalAsync(
            MaraHollyOrderLineDto? order,
            string newStatus)
        {
            if (order == null)
                return;

            try
            {
                IsBusy = true;
                ErrorMessage = null;

                // Persist change to local DB
                await _orderService.UpdateProcessingStatusAsync(order.Id, newStatus);

                // After updating in DB, reload everything so the UI reflects the latest state
                await LoadOrdersAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ---------------------------------
        // Property change hooks
        // ---------------------------------

        partial void OnSearchTextChanged(string? value)
        {
            ApplyFilters();
        }

        partial void OnSelectedProcessingStatusFilterChanged(string? value)
        {
            ApplyFilters();
        }

        partial void OnLastSyncTimeChanged(DateTime? value)
        {
            OnPropertyChanged(nameof(LastSyncLabel));
        }
    }
}
