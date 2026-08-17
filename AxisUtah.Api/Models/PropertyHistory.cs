namespace AxisUtah.Api.Models;

public enum PropertyFieldType
{
    ListPrice,
    StandardStatus,
    PublicRemarks,
    ListingAgentId,
    ListingBrokerageId
}

public class PropertyHistory
{
    [Key]
    public long Id { get; set; }

    [ForeignKey(nameof(Property))]
    public int PropertyId { get; set; }
    public Property? Property { get; set; }

    public PropertyFieldType FieldType { get; set; }
    
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }

    public DateTimeOffset ChangedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public string? CorrelationId { get; set; }
    public string Source { get; set; } = "ODataSync";
}