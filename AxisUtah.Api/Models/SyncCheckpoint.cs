namespace AxisUtah.Api.Models;

public class SyncCheckpoint
{
    public const string PropertyFeedName = "Property";

    [Key]
    public string FeedName { get; set; } = string.Empty;

    public DateTimeOffset? LastModificationTimestamp { get; set; }
    public DateTimeOffset? LastRunStartedAt { get; set; }
    public DateTimeOffset? LastRunCompletedAt { get; set; }
    public string? LastError { get; set; }
}