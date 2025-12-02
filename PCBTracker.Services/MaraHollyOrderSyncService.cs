using Microsoft.EntityFrameworkCore;
using PCBTracker.Data.Context;
using PCBTracker.Domain.DTOs;
using PCBTracker.Domain.Entities;
using PCBTracker.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PCBTracker.Services
{
    public class MaraHollyOrderSyncService : IMaraHollyOrderSyncService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly IMaraHollyOrdersODataClient _odataClient;

        public MaraHollyOrderSyncService(
            IDbContextFactory<AppDbContext> contextFactory,
            IMaraHollyOrdersODataClient odataClient)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _odataClient = odataClient ?? throw new ArgumentNullException(nameof(odataClient));
        }

        public async Task<MaraHollyOrderSyncResult> SyncAsync(CancellationToken cancellationToken = default)
        {
            var result = new MaraHollyOrderSyncResult();

            // 1. Pull current GI snapshot from Acumatica
            var rows = await _odataClient.GetMaraHollyOrdersAsync(cancellationToken);

            using var db = await _contextFactory.CreateDbContextAsync(cancellationToken);

            foreach (var row in rows)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Defensive: skip garbage rows that somehow have no order number
                var orderNbr = row.OrderNbr?.Trim();
                if (string.IsNullOrWhiteSpace(orderNbr))
                {
                    result.Skipped++;
                    continue;
                }

                var lineNbr = row.LineNbr;

                // Parse / normalize OData fields
                var customerId = (row.Customer ?? string.Empty).Trim();
                var inventoryId = (row.InventoryID ?? string.Empty).Trim();
                var status = (row.Status ?? string.Empty).Trim();
                var customerOrderNbr = string.IsNullOrWhiteSpace(row.CustomerOrderNbr)
                    ? null
                    : row.CustomerOrderNbr.Trim();

                var requestDate = ParseDate(row.RequestedOn);
                var orderQty = ParseDecimal(row.Quantity);
                var openQty = ParseDecimal(row.OpenQty);

                // 2. Try to find an existing local line by natural key (OrderNbr + LineNbr)
                var existing = await db.MaraHollyOrderLine
                    .SingleOrDefaultAsync(
                        x => x.OrderNbr == orderNbr && x.LineNbr == lineNbr,
                        cancellationToken);

                if (existing is null)
                {
                    // 3a. Insert new row
                    var entity = new MaraHollyOrderLine
                    {
                        CustomerId = customerId,
                        OrderNbr = orderNbr,
                        CustomerOrderNbr = customerOrderNbr,
                        LineNbr = lineNbr,
                        InventoryId = inventoryId,
                        OrderQty = orderQty,
                        OpenQty = openQty,
                        RequestDate = requestDate,
                        Status = status,
                        ScannedQty = openQty,
                        ProcessingStatus = "Unassigned",
                        LastUpdatedAt = DateTime.UtcNow
                    };

                    db.MaraHollyOrderLine.Add(entity);
                    result.Inserted++;
                }
                else
                {
                    // 3b. Update Acumatica-driven fields if they changed
                    bool changed = false;

                    if (!string.Equals(existing.CustomerId, customerId, StringComparison.Ordinal))
                    {
                        existing.CustomerId = customerId;
                        changed = true;
                    }

                    if (!string.Equals(existing.CustomerOrderNbr, customerOrderNbr, StringComparison.Ordinal))
                    {
                        existing.CustomerOrderNbr = customerOrderNbr;
                        changed = true;
                    }

                    if (!string.Equals(existing.InventoryId, inventoryId, StringComparison.Ordinal))
                    {
                        existing.InventoryId = inventoryId;
                        changed = true;
                    }

                    if (existing.OrderQty != orderQty)
                    {
                        existing.OrderQty = orderQty;
                        changed = true;
                    }

                    if (existing.OpenQty != openQty)
                    {
                        existing.OpenQty = openQty;
                        changed = true;
                    }

                    if (!NullableDateEquals(existing.RequestDate, requestDate))
                    {
                        existing.RequestDate = requestDate;
                        changed = true;
                    }

                    if (!string.Equals(existing.Status, status, StringComparison.Ordinal))
                    {
                        existing.Status = status;
                        changed = true;
                    }

                    if (changed)
                    {
                        existing.LastUpdatedAt = DateTime.UtcNow;
                        result.Updated++;
                    }
                    else
                    {
                        result.Unchanged++;
                    }

                    // NOTE: We deliberately do NOT touch existing.ProcessingStatus here.
                }
            }

            await db.SaveChangesAsync(cancellationToken);
            return result;
        }

        // Helpers -----------------------

        private static DateTime? ParseDate(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            // Typical GI date format: "2025-11-10T00:00:00"
            if (DateTime.TryParse(
                    value,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out var dt))
            {
                return dt;
            }

            if (DateTime.TryParse(value, out dt))
                return dt;

            return null;
        }

        private static decimal ParseDecimal(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0m;

            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                return d;

            if (decimal.TryParse(value, out d))
                return d;

            return 0m;
        }

        private static bool NullableDateEquals(DateTime? a, DateTime? b)
        {
            if (!a.HasValue && !b.HasValue) return true;
            if (!a.HasValue || !b.HasValue) return false;

            // Compare by DateTime value; if you want to ignore time-of-day, adjust here.
            return a.Value == b.Value;
        }
    }
}
