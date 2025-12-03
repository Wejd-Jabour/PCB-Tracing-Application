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

            // Upper cutoff: today + 30 days
            var upperCutoff = DateTime.UtcNow.Date.AddDays(days);

            var query = db.MaraHollyOrderLine
                .AsNoTracking()
                .Where(x => x.RequestDate <= upperCutoff);

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
                    ScannedQty = x.ScannedQty,
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


        public async Task IncrementScannedQtyAsync(
    int id,
    decimal incrementBy,
    CancellationToken cancellationToken = default)
        {
            await using var db = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var entity = await db.MaraHollyOrderLine
                .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (entity is null)
            {
                // Option: just return silently, or log if you have logging
                // e.g. Debug.WriteLine($"Order line {id} not found when incrementing scan qty.");
                return;
            }

            entity.ScannedQty += incrementBy;
            entity.LastUpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task<MaraHollyOrderLineDto> GetOrderByIdAsync(
    int id,
    CancellationToken cancellationToken = default)
        {
            using var db = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var entity = await db.MaraHollyOrderLine
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (entity is null)
                return null;

            return new MaraHollyOrderLineDto
            {
                Id = entity.Id,
                CustomerId = entity.CustomerId,
                OrderNbr = entity.OrderNbr,
                CustomerOrderNbr = entity.CustomerOrderNbr,
                LineNbr = entity.LineNbr,
                InventoryId = entity.InventoryId,
                OrderQty = entity.OrderQty,
                OpenQty = entity.OpenQty,
                ScannedQty = entity.ScannedQty,
                RequestDate = entity.RequestDate,
                Status = entity.Status,
                ProcessingStatus = entity.ProcessingStatus,
                LastUpdatedAt = entity.LastUpdatedAt
            };
        }
    }
}
