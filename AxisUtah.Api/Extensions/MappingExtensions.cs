namespace AxisUtah.Api.Extensions;

public static class MappingExtensions
{
    public static PropertyResponseDto ToPropertyResponseDto(this Property property)
    {
        return new PropertyResponseDto
        {
            PropertyId = property.PropertyId,
            ListingKey = property.ListingKey,
            ListingId = property.ListingId,
            ListPrice = property.ListPrice,
            BedroomsTotal = property.BedroomsTotal,
            BathroomsTotal = property.BathroomsTotal,
            BuildingAreaTotal = property.BuildingAreaTotal,
            StandardStatus = property.StandardStatus,
            PropertyType = property.PropertyType,
            StructureType = property.StructureType,
            PublicRemarks = property.PublicRemarks,
            UnparsedAddress = property.UnparsedAddress,
            City = property.City,
            StateOrProvince = property.StateOrProvince,
            PostalCode = property.PostalCode,
            Latitude = property.Latitude,
            Longitude = property.Longitude,
            ListAgentFullName = property.ListAgentFullName,
            ListOfficeName = property.ListOfficeName,
            IsBrokerageListing = property.IsBrokerageListing,
            IsActive = property.IsActive,
            ModificationTimestamp = property.ModificationTimestamp,
            LastSyncedAt = property.LastSyncedAt,
            Media = [.. property.Media.Select(m => new PropertyMediaDto
            {
                MediaUrl = m.MediaUrl,
                Order = m.Order
            })]
        };
    }
}