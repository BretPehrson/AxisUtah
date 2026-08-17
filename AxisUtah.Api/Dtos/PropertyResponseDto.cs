namespace AxisUtah.Api.Dtos;

public class PropertyResponseDto
{
    public int PropertyId { get; set; }
    public int ListingKey { get; set; }
    public string? ListingId { get; set; }
    public decimal? ListPrice { get; set; }
    public int? BedroomsTotal { get; set; }
    public decimal? BathroomsTotal { get; set; }
    public int? BuildingAreaTotal { get; set; }
    public string? StandardStatus { get; set; }
    public string? PropertyType { get; set; }
    public string? StructureType { get; set; }
    public string? PublicRemarks { get; set; }
    public string? UnparsedAddress { get; set; }
    public string? City { get; set; }
    public string? StateOrProvince { get; set; }
    public string? PostalCode { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? ListAgentFullName { get; set; }
    public string? ListOfficeName { get; set; }
    public bool IsBrokerageListing { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset ModificationTimestamp { get; set; }
    public DateTimeOffset LastSyncedAt { get; set; }
    public List<PropertyMediaDto> Media { get; set; } = [];
}

public class PropertyMediaDto
{
    public string MediaUrl { get; set; } = string.Empty;
    public int Order { get; set; }
}