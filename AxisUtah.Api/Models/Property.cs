namespace AxisUtah.Api.Models;

public class Property
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PropertyId { get; set; }

    [Required]
    public int ListingKey { get; set; }

    public string? ListingId { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal? ListPrice { get; set; }
    public int? BedroomsTotal { get; set; }    
    [Column(TypeName = "decimal(4,1)")]
    public decimal? BathroomsTotal { get; set; }
    public int? BuildingAreaTotal { get; set; }
    public string? StandardStatus { get; set; } = "Active";
    public string? PropertyType { get; set; }
    public string? StructureType { get; set; }
    public string? PublicRemarks { get; set; }
    public string? UnparsedAddress { get; set; }
    public string? City { get; set; }
    public string? StateOrProvince { get; set; } = "UT";
    public string? PostalCode { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? ListAgentFullName { get; set; }
    public string? ListOfficeName { get; set; }
    public bool IsBrokerageListing { get; set; }
    public bool IsActive { get; set; }

    [ForeignKey(nameof(Address))]
    public int? AddressId { get; set; }
    public Address? Address { get; set; }

    [ForeignKey(nameof(Agent))]
    public int? AgentId { get; set; }
    public Agent? Agent { get; set; }

    [ForeignKey(nameof(Brokerage))]
    public int? BrokerageId { get; set; }
    public Brokerage? Brokerage { get; set; }

    // RESO Sync Metadata
    public DateTimeOffset ModificationTimestamp { get; set; }
    public DateTimeOffset LastSyncedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation Property for CDN Images
    public List<PropertyMedia> Media { get; set; } = new();
}

public class SavedProperty
{
    public int UserId { get; set; }
    public int PropertyId { get; set; }
    public bool Active { get; set; } = true;

    public User User { get; set; } = null!;
    public Property Property { get; set; } = null!;
}

public class PropertyMedia
{
    public int Id { get; set; }
    
    public int PropertyId { get; set; }
    
    [ForeignKey(nameof(PropertyId))]
    public Property? Property { get; set; }
                
    public string MediaUrl { get; set; } = string.Empty;
    public int Order { get; set; }
}

public class Lead
{
    public int Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Message { get; set; }

    public int? PropertyId { get; set; } // Nullable if general inquiry
    [ForeignKey(nameof(PropertyId))]
    public Property? Property { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}