using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PCBTracker.Domain.DTOs
{
    // Root object for the OData response
    public class MaraHollyOrdersODataResponse
    {
        [JsonPropertyName("value")]
        public List<MaraHollyODataRow> Value { get; set; } = new();
    }

    // Single row from the MaraHollyOrders GI
    public class MaraHollyODataRow
    {
        public string OrderNbr { get; set; } = default!;
        public string Customer { get; set; } = default!;
        public string? CustomerOrderNbr { get; set; }
        public string InventoryID { get; set; } = default!;
        public string RequestedOn { get; set; } = default!; // keep as string, we’ll parse later
        public string Status { get; set; } = default!;
        public string Quantity { get; set; } = default!;
        public string OpenQty { get; set; } = default!;
        public string OrderType { get; set; } = default!;
        public string OrderType_2 { get; set; } = default!;
        public string OrderNbr_2 { get; set; } = default!;
        public int LineNbr { get; set; }
    }
}
