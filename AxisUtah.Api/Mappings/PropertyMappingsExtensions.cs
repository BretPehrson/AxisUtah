namespace AxisUtah.Api.Mappings;

public static class PropertyMappingsExtensions
{
    public static Property ToEntity(this ResoPropertyDto dto)
    {
        return new Property
        {
            ListingKey = dto.ListingKey,
            ListingId = dto.ListingId,
            ListPrice = dto.ListPrice,
            BedroomsTotal = dto.BedroomsTotal,
            BathroomsTotal = dto.BathroomsTotalInteger,
            BuildingAreaTotal = dto.BuildingAreaTotal,
            StandardStatus = dto.StandardStatus,
            PropertyType = dto.PropertyType,
            StructureType = dto.StructureType,
            PublicRemarks = dto.PublicRemarks,
            UnparsedAddress = dto.UnparsedAddress,
            City = dto.City,
            StateOrProvince = dto.StateOrProvince,
            PostalCode = dto.PostalCode,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            ListAgentFullName = dto.ListAgentFullName,
            ListOfficeName = dto.ListOfficeName,
            IsBrokerageListing = dto.ListOfficeName?.Contains("Axis Realty", StringComparison.OrdinalIgnoreCase) ?? false,
            ModificationTimestamp = dto.ModificationTimestamp,
            Media = dto.Media?.Select((m, index) => new PropertyMedia
            {
                ListingKey = dto.ListingKey,
                MediaUrl = m.MediaURL,
                Order = m.Order ?? index
            }).ToList() ?? []
        };
    }

    public static void UpdateEntityFromDto(this Property entity, ResoPropertyDto dto)
    {
        entity.ListingId = dto.ListingId;
        entity.ListPrice = dto.ListPrice;
        entity.BedroomsTotal = dto.BedroomsTotal;
        entity.BathroomsTotal = dto.BathroomsTotalInteger;
        entity.BuildingAreaTotal = dto.BuildingAreaTotal;
        entity.StandardStatus = dto.StandardStatus;
        entity.PropertyType = dto.PropertyType;
        entity.StructureType = dto.StructureType;
        entity.PublicRemarks = dto.PublicRemarks;
        entity.UnparsedAddress = dto.UnparsedAddress;
        entity.City = dto.City;
        entity.StateOrProvince = dto.StateOrProvince;
        entity.PostalCode = dto.PostalCode;
        entity.Latitude = dto.Latitude;
        entity.Longitude = dto.Longitude;
        entity.ListAgentFullName = dto.ListAgentFullName;
        entity.ListOfficeName = dto.ListOfficeName;
        entity.IsBrokerageListing = dto.ListOfficeName?.Contains("Axis Realty", StringComparison.OrdinalIgnoreCase) ?? false;
        entity.ModificationTimestamp = dto.ModificationTimestamp;
        entity.LastSyncedAt = DateTimeOffset.UtcNow;
        
        // Re-map media URLs
        entity.Media.Clear();
        entity.Media = dto.Media?.Select((m, index) => new PropertyMedia
        {
            ListingKey = dto.ListingKey,
            MediaUrl = m.MediaURL,
            Order = m.Order ?? index
        }).ToList() ?? [];
    }
}