// PCBTracker.Domain/DTOs/BoardDto.cs
using System;

namespace PCBTracker.Domain.DTOs
{
    /// <summary>
    /// Data Transfer Object for creating a new Board.
    /// </summary>
    public class BoardDto
    {
        public string SerialNumber { get; set; } = default!;
        public string PartNumber { get; set; } = default!;
        public string BoardType { get; set; } = default!;
        public DateTime PrepDate { get; set; }
        public bool IsShipped { get; set; }
        public DateTime? ShipDate { get; set; }
        public int SkidID { get; set; }
    }
}
