namespace AxisUtah.Api.Models;

public enum LockboxStatus
{
    InOffice,
    Deployed,
    Missing,
    Damaged
}

public enum OwnerType
{
    Brokerage,
    AgentOwned
}

public class Lockbox
{
    [Key]
    [StringLength(20)]
    public string SerialNumber { get; set; } = null!;

    [Required]
    public LockboxStatus Status { get; set; } = LockboxStatus.InOffice;

    [Required]
    public OwnerType OwnerType { get; set; } = OwnerType.Brokerage;

    // Tracks who owns the box (if Agent Owned) OR who currently has it checked out
    public int? AssignedAgentId { get; set; }
    public Agent? AssignedAgent { get; set; }

    // Manually typed or matched from your local URE listing cache
    [StringLength(50)]
    public string? CurrentMlsNumber { get; set; }

    [StringLength(200)]
    public string? PhysicalLocationNotes { get; set; } // e.g., "Back gas meter", "Desk Drawer 2"

    public DateTime LastAuditedAt { get; set; } = DateTime.UtcNow;
    public string? UpdatedBy { get; set; } // Tracks which admin user last saved changes
}