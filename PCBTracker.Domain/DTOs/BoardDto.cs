using System;

namespace PCBTracker.Domain.DTOs
{
    /// <summary>
    /// Represents a data transfer object (DTO) for board submission and transport.
    /// This object is used to encapsulate board-related data across layers
    /// without directly exposing the underlying database entity.
    /// </summary>
    public class BoardDto
    {
        /// <summary>
        /// The unique 16-character serial number assigned to the board.
        /// This value is entered or scanned by the user and used as a primary identifier.
        /// The service layer enforces uniqueness before persisting to the database.
        /// </summary>
        public string SerialNumber { get; set; } = default!;

        /// <summary>
        /// The part number that identifies the board's model, revision, or specification.
        /// This is often mapped from the selected board type.
        /// </summary>
        public string PartNumber { get; set; } = default!;

        /// <summary>
        /// The type of the board, such as "LE", "SAD", "SAT Upgrade".
        /// Used to determine the appropriate destination table and processing rules.
        /// </summary>
        public string BoardType { get; set; } = default!;

        /// <summary>
        /// The date on which the board was prepared.
        /// Used for filtering and reporting; this field is required.
        /// </summary>
        public DateTime PrepDate { get; set; }

        /// <summary>
        /// Indicates whether the board has been marked as shipped.
        /// If true, a corresponding ShipDate value may be required.
        /// </summary>
        public bool IsShipped { get; set; }

        /// <summary>
        /// The date the board was shipped, if applicable.
        /// Only populated when IsShipped is true. May default to PrepDate if not explicitly set.
        /// </summary>
        public DateTime? ShipDate { get; set; }

        /// <summary>
        /// The ID of the Skid (container) this board is assigned to.
        /// This value links the board to a grouping unit in the system.
        /// </summary>
        public int SkidID { get; set; }
    }
}
