using PCBTracker.Domain.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PCBTracker.Services.Interfaces
{
    public interface IMaraHollyOrderService
    {
        /// <summary>
        /// Returns MaraHolly order lines from the local DB whose RequestDate is
        /// within the last <paramref name="days"/> days (default 30), ordered by
        /// RequestDate, OrderNbr, LineNbr.
        /// </summary>
        Task<IReadOnlyList<MaraHollyOrderLineDto>> GetRecentOrdersAsync(
            int days = 30,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the ProcessingStatus of a single order line by Id.
        /// Allowed statuses: Unassigned, Active, OnHold, Cancelled.
        /// </summary>
        Task UpdateProcessingStatusAsync(
            int id,
            string newStatus,
            CancellationToken cancellationToken = default);
    }
}
