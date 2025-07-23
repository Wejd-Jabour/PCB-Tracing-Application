namespace PCBTracker.Domain.DTOs
{
    public class BoardFilterDto
    {
        public string? SerialNumber { get; set; }
        public string? BoardType { get; set; }
        public DateTime? PrepDateFrom { get; set; }
        public DateTime? PrepDateTo { get; set; }
        public DateTime? ShipDateFrom { get; set; }
        public DateTime? ShipDateTo { get; set; }
        public int? SkidId { get; set; }
    }
}
