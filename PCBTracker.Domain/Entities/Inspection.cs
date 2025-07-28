using System;

namespace PCBTracker.Domain.Entities
{
    public class Inspection
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }
        public string ProductType { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string IssueDescription { get; set; } = string.Empty;
        public string SeverityLevel { get; set; } = "Unknown";
        public string ImmediateActionTaken { get; set; } = string.Empty;
        public string AdditionalNotes { get; set; } = string.Empty;

        // JSON serialized assemblies map
        public string AssembliesCompletedJson { get; set; } = "{}";

        public DateTime CreatedAt { get; set; }
    }
}
