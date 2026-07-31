namespace AxisUtah.Api.Models;

public class AppLogEntry
{
    public long Id { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public string Level { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string? CorrelationId { get; set; }
    public string? DetailsJson { get; set; }
}