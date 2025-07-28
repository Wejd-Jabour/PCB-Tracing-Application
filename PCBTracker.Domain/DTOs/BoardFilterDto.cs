namespace PCBTracker.Domain.DTOs
{
    /// <summary>
    /// Data Transfer Object used for filtering and paginating board records during queries.
    /// All properties are optional and used to construct dynamic filters.
    /// </summary>
    public class BoardFilterDto
    {
        /// <summary>
        /// Optional filter by serial number.
        /// If provided, boards matching this string (typically using Contains) are included.
        /// </summary>
        public string? SerialNumber { get; set; }

        /// <summary>
        /// Optional filter by board type.
        /// Filters the result set to only include boards of the specified type.
        /// </summary>
        public string? BoardType { get; set; }

        /// <summary>
        /// Lower bound for filtering by preparation date (inclusive).
        /// Used only if UseShipDate is false in the context where this filter is applied.
        /// </summary>
        public DateTime? PrepDateFrom { get; set; }

        /// <summary>
        /// Upper bound for filtering by preparation date (inclusive).
        /// Used only if UseShipDate is false in the context where this filter is applied.
        /// </summary>
        public DateTime? PrepDateTo { get; set; }

        /// <summary>
        /// Lower bound for filtering by ship date (inclusive).
        /// Used only if UseShipDate is true in the context where this filter is applied.
        /// </summary>
        public DateTime? ShipDateFrom { get; set; }

        /// <summary>
        /// Upper bound for filtering by ship date (inclusive).
        /// Used only if UseShipDate is true in the context where this filter is applied.
        /// </summary>
        public DateTime? ShipDateTo { get; set; }

        /// <summary>
        /// Optional filter by Skid ID.
        /// If provided, restricts results to boards assigned to the specified Skid.
        /// </summary>
        public int? SkidId { get; set; }

        /// <summary>
        /// The index of the page to retrieve during pagination.
        /// Page indexing is 1-based.
        /// </summary>
        public int? PageNumber { get; set; }

        /// <summary>
        /// The number of items to return per page during pagination.
        /// Used in conjunction with PageNumber.
        /// </summary>
        public int? PageSize { get; set; }

        /// <summary>
        /// Indicates whether the board has been marked as shipped.
        /// If true, a corresponding ShipDate value may be required.
        /// </summary>
        public bool? IsShipped { get; set; }

    }
}
