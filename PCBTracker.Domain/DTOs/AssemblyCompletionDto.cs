using System;

namespace PCBTracker.Domain.DTOs
{
    public class AssemblyCompletionDto
    {
        public DateTime Date { get; set; }
        public string BoardType { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
