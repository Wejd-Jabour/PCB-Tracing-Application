using Microsoft.EntityFrameworkCore;
using PCBTracker.Data.Context;
using PCBTracker.Domain.DTOs;
using PCBTracker.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PCBTracker.Services
{
    public class MaraHollyOrderService : IMaraHollyOrderService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        private static readonly string[] AllowedProcessingStatuses =
        {
            "Unassigned",
            "Active",
            "OnHold",
            "Cancelled",
            "Complete",
            "Void"
        };



        public MaraHollyOrderService(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        public async Task<IReadOnlyList<MaraHollyOrderLineDto>> GetRecentOrdersAsync(
            int days = 30,
            CancellationToken cancellationToken = default)
        {
            using var db = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var cutoff = DateTime.UtcNow.Date.AddDays(-days);

            var query = db.MaraHollyOrderLine
                .AsNoTracking()
                .Where(x => x.RequestDate >= cutoff);

            var list = await query
                .OrderBy(x => x.RequestDate)
                .ThenBy(x => x.OrderNbr)
                .ThenBy(x => x.LineNbr)
                .Select(x => new MaraHollyOrderLineDto
                {
                    Id = x.Id,
                    CustomerId = x.CustomerId,
                    OrderNbr = x.OrderNbr,
                    CustomerOrderNbr = x.CustomerOrderNbr,
                    LineNbr = x.LineNbr,
                    InventoryId = x.InventoryId,
                    OrderQty = x.OrderQty,
                    OpenQty = x.OpenQty,
                    RequestDate = x.RequestDate,
                    Status = x.Status,
                    ProcessingStatus = x.ProcessingStatus,
                    LastUpdatedAt = x.LastUpdatedAt
                })
                .ToListAsync(cancellationToken);

            return list;
        }

        public async Task UpdateProcessingStatusAsync(
            int id,
            string newStatus,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(newStatus))
                throw new ArgumentException("Processing status is required.", nameof(newStatus));

            if (!AllowedProcessingStatuses.Contains(newStatus, StringComparer.OrdinalIgnoreCase))
                throw new ArgumentOutOfRangeException(
                    nameof(newStatus),
                    $"Invalid processing status '{newStatus}'. " +
                    $"Allowed: {string.Join(", ", AllowedProcessingStatuses)}.");

            using var db = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var entity = await db.MaraHollyOrderLine
                .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (entity is null)
                throw new InvalidOperationException($"MaraHollyOrderLine with Id {id} was not found.");

            // Normalize casing to the canonical value
            var normalized = AllowedProcessingStatuses
                .First(x => x.Equals(newStatus, StringComparison.OrdinalIgnoreCase));

            entity.ProcessingStatus = normalized;

            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
