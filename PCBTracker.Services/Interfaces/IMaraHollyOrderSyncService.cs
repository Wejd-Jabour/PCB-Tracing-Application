using PCBTracker.Domain.DTOs;
using System.Threading;
using System.Threading.Tasks;

namespace PCBTracker.Services.Interfaces
{
    /// <summary>
    /// Synchronizes MaraHolly order lines from Acumatica's OData GI into the local SQL database.
    /// </summary>
    public interface IMaraHollyOrderSyncService
    {
        /// <summary>
        /// Pulls the latest MaraHollyOrders from Acumatica and upserts local rows.
        /// - Inserts new lines.
        /// - Updates existing lines when Acumatica data changed (and bumps LastUpdatedAt).
        /// - Preserves ProcessingStatus choices made by the coordinator.
        /// </summary>
        Task<MaraHollyOrderSyncResult> SyncAsync(CancellationToken cancellationToken = default);
    }
}
