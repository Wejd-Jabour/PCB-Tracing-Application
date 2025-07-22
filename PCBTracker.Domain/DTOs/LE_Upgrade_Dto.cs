using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCBTracker.Domain.DTOs
{
    internal class LE_Upgrade_Dto
    {
        // The unique serial number scanned or entered for this board.
        // Required and validated by the service layer to ensure uniqueness.
        public string SerialNumber { get; set; } = default!;

        // The manufacturer or internal part number associated with this board.
        // Helps identify the board's variant or revision.
        public string PartNumber { get; set; } = default!;

        // The type/category of the board (e.g., "LE", "SAD", "SAT Upgrade").
        // Populated from a lookup in the SubmitViewModel via IBoardService.
        public string BoardType { get; set; } = default!;

        // The date the board was prepared or scanned into the system.
        // Bound to a DatePicker in the Submit page.
        public DateTime PrepDate { get; set; }

        // Flag indicating whether the board has been marked as shipped.
        // If true, ShipDate must be populated (defaults to PrepDate if left blank).
        public bool IsShipped { get; set; }

        // Optional date the board was or will be shipped.
        // Only has a value when IsShipped is true.
        public DateTime? ShipDate { get; set; }

        // Foreign key linking to the Skid on which this board is placed.
        // Determined by the SubmitViewModel, which picks or creates the next skid.
        public int SkidID { get; set; }
    }
}
