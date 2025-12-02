using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCBTracker.Domain.Entities
{
    public class MaraHollyOrderLine
    {
        [Key]

        public int Id { get; set; }
        public string CustomerId { get; set; } = default!;
        public string OrderNbr { get; set; } = default!;
        public string? CustomerOrderNbr { get; set; }
        public int LineNbr { get; set; }
        public string InventoryId { get; set; } = default!;
        public decimal OrderQty { get; set; }
        public decimal OpenQty { get; set; }
        public DateTime? RequestDate { get; set; }
        public decimal ScannedQty { get; set; }
        public string Status { get; set; } = default!;
        public string ProcessingStatus { get; set; } = "Unassigned";
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
