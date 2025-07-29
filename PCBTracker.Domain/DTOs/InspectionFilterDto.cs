// InspectionFilterDto.cs
using System;

namespace PCBTracker.Domain.DTOs
{
    public class InspectionFilterDto
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? ProductType { get; set; }
        public string? SerialNumberContains { get; set; }
        public string? SeverityLevel { get; set; }
        public int? PageNumber { get; set; }
        public int? PageSize { get; set; }
    }
}