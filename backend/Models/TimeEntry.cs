public class TimeEntry
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public decimal Quantity { get; set; }
    public string? Notes { get; set; }
    public decimal Multiplier { get; set; } = 1m; // default
}
