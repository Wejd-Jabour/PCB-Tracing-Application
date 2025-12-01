using PCBTracker.Domain.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PCBTracker.Services.Interfaces
{
    public interface IMaraHollyOrdersODataClient
    {
        /// <summary>
        /// Calls the MaraHollyOrders OData endpoint and returns all rows
        /// that the GI currently exposes (30 days, Open/Back Order, etc.).
        /// </summary>
        Task<IReadOnlyList<MaraHollyODataRow>> GetMaraHollyOrdersAsync(
            CancellationToken cancellationToken = default);
    }
}
