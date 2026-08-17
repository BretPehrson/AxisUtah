namespace AxisUtah.Api.Dtos;

// Support DTOs matching RESO Standard JSON response
// RESO = Real Estate Standards Organization
public record ResoODataResponse(
    [property: JsonPropertyName("value")] List<ResoPropertyDto> Value,
    [property: JsonPropertyName("@odata.nextLink")] string? NextLink = null
);

public record ResoPropertyDto(
    int ListingKey,
    string? ListingId,
    decimal? ListPrice,
    int? BedroomsTotal,
    decimal? BathroomsTotalInteger,
    int? BuildingAreaTotal,
    string? StandardStatus,
    string? PropertyType,
    string? StructureType,
    string? PublicRemarks,
    string? UnparsedAddress,
    string? City,
    string? StateOrProvince,
    string? PostalCode,
    double? Latitude,
    double? Longitude,
    string? ListAgentFullName,
    string? ListOfficeName,
    DateTimeOffset ModificationTimestamp,
    List<ResoMediaDto>? Media
);

public record ResoMediaDto(string MediaUrl, int? Order = null);