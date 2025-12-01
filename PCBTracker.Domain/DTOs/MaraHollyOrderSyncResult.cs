using System;

namespace PCBTracker.Domain.DTOs
{
    /// <summary>
    /// Summary of a sync run between Acumatica's MaraHollyOrders GI and the local SQL DB.
    /// </summary>
    public sealed class MaraHollyOrderSyncResult
    {
        public int Inserted { get; set; }
        public int Updated { get; set; }
        public int Unchanged { get; set; }
        public int Skipped { get; set; }
    }
}
