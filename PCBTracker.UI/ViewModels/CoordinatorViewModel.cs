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

        public CoordinatorViewModel(
            IMaraHollyOrderService orderService,
            IMaraHollyOrderSyncService syncService)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _syncService = syncService ?? throw new ArgumentNullException(nameof(syncService));

            ProcessingStatusOptions = new ObservableCollection<string>(
                new[] { "All", "Unassigned", "Active", "OnHold", "Cancelled" });

            SelectedProcessingStatusFilter = ProcessingStatusOptions.FirstOrDefault();
        }

        // -----------------------------
        // Collections / state
        // -----------------------------

        [ObservableProperty]
        private ObservableCollection<MaraHollyOrderLineDto> orders = new();

        [ObservableProperty]
        private ObservableCollection<string> processingStatusOptions;

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
                var result = await _syncService.SyncAsync();

                // You could log or surface result.Inserted/Updated here if you like
                LastSyncTime = DateTime.UtcNow;

                // 2. Reload from local DB
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

        // Status change commands (for swipe actions / buttons)

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

            // ProcessingStatus filter
            if (!string.IsNullOrWhiteSpace(SelectedProcessingStatusFilter) &&
                !string.Equals(SelectedProcessingStatusFilter, "All", StringComparison.OrdinalIgnoreCase))
            {
                var status = SelectedProcessingStatusFilter;

                query = query.Where(o =>
                    string.Equals(o.ProcessingStatus, status, StringComparison.OrdinalIgnoreCase));
            }

            // Order by request date, then OrderNbr, then LineNbr
            query = query
                .OrderBy(o => o.RequestDate ?? DateTime.MaxValue)
                .ThenBy(o => o.OrderNbr)
                .ThenBy(o => o.LineNbr);

            Orders.Clear();
            foreach (var item in query)
            {
                Orders.Add(item);
            }

            OnPropertyChanged(nameof(HasOrders));
        }

        private async Task SetProcessingStatusInternalAsync(
            MaraHollyOrderLineDto? order,
            string newStatus)
        {
            if (order == null)
                return;

            try
            {
                ErrorMessage = null;

                await _orderService.UpdateProcessingStatusAsync(order.Id, newStatus);

                // After updating in DB, reload everything so the UI reflects the latest state
                await LoadOrdersAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
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
