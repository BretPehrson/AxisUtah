namespace AxisUtah.Api.Models;

public class Agent
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int AgentId { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public bool IsActive { get; set; } = true;

    [ForeignKey(nameof(Brokerage))]
    public int? BrokerageId { get; set; }
    public Brokerage? Brokerage { get; set; }
}