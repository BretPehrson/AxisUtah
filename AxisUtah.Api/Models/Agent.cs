namespace AxisUtah.Api.Models;

public class Agent
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int AgentId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public bool IsActive { get; set; } = true;

    [ForeignKey(nameof(Brokerage))]
    public int? BrokerageId { get; set; }
    public Brokerage? Brokerage { get; set; }

    [ForeignKey(nameof(User))]
    public int? UserId { get; set; }
    public User? User { get; set; }

    public List<Lockbox> Lockboxes { get; set; } = [];
}