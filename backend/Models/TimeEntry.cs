public class TimeEntry
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Computed — not stored in DB
    public TimeSpan? Duration => EndTime.HasValue ? EndTime - StartTime : null;
}
