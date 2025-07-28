using System;
using System.Collections.Generic;

namespace PCBTracker.Domain.DTOs
{
    public class InspectionDto
    {
        public DateTime Date { get; set; }
        public string ProductType { get; set; } = string.Empty; // "LE", "SAT", etc.
        public string SerialNumber { get; set; } = string.Empty; // Barcode
        public string IssueDescription { get; set; } = string.Empty;
        public string SeverityLevel { get; set; } = "Unknown"; // Minor, Moderate, High, Unknown
        public string ImmediateActionTaken { get; set; } = string.Empty;
        public string AdditionalNotes { get; set; } = string.Empty;

        // AssembliesCompleted is a dictionary of product to count.
        public Dictionary<string, int> AssembliesCompleted { get; set; } = new();
    }
}
