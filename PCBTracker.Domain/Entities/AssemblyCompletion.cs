namespace PCBTracker.Domain.Entities
{
    public class AssemblyCompletion
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string BoardType { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
