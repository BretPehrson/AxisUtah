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

public sealed class PropertySearchRequest
{
    public string? Search { get; init; }

    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }

    public int? MinBedrooms { get; init; }
    public decimal? MinBathrooms { get; init; }
    public int? MinBuildingArea { get; init; }

    public string? City { get; init; }
    public string? StateOrProvince { get; init; }
    public string? PostalCode { get; init; }
    public string? PropertyType { get; init; }
    public string? StandardStatus { get; init; }

    public bool? BrokerageOnly { get; init; }

    public int Page { get; init; } = 1;
    public int PageSize { get; init; } =  20;
    public string SortBy { get; init; } = "ModificationTimestamp";
    public bool SortDescending { get; init; } = true;
}

public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
}