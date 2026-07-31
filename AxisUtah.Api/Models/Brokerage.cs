namespace AxisUtah.Api.Models;

public class Brokerage
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int BrokerageId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? StateOrProvince { get; set; } = "UT";
    public string? PostalCode { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public bool IsActive { get; set; } = true;
}