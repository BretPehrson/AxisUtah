namespace AxisUtah.Api.Services;

public class PropertyService(IDbContextFactory<AppDbContext> dbContextFactory)
{
    private IDbContextFactory<AppDbContext> Context { get; } = dbContextFactory;

    #region User Saved Properties

    public async Task<IEnumerable<PropertyResponseDto>> GetAllUserSavedProperties(int? userId)
    {
        using var context = Context.CreateDbContext();
        var properties = await context.SavedProperties
            .Where(sp => sp.UserId == userId && sp.Active)
            .Include(sp => sp.Property)
            .ThenInclude(p => p.Media)
            .Select(sp => sp.Property)
            .ToListAsync();

        return properties.Select(p => p.ToPropertyResponseDto());
    }

    public async Task<PropertyResponseDto?> GetUserSavedPropertyById(int? userId, int propertyId)
    {
        using var context = Context.CreateDbContext();
        var property = await context.SavedProperties
            .Where(sp => sp.UserId == userId && sp.Active)
            .Include(sp => sp.Property)
            .ThenInclude(sp => sp.Media)
            .Select(sp => sp.Property)
            .FirstOrDefaultAsync(p => p.PropertyId == propertyId);
        return property?.ToPropertyResponseDto();
    }

    public async Task<bool> SaveUserProperty(int? userId, int propertyId)
    {
        using var context = Context.CreateDbContext();

        var savedProperty = await context.SavedProperties
            .FirstOrDefaultAsync(sp => sp.UserId == userId && sp.PropertyId == propertyId);
        if (savedProperty == null)
        {
            context.SavedProperties.Add(new SavedProperty
            {
                UserId = userId!.Value,
                PropertyId = propertyId,
                Active = true
            });
        }
        else
        {
            savedProperty.Active = true;
        }

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteUserSavedProperty(int? userId, int propertyId)
    {
        using var context = Context.CreateDbContext();
        var savedProperty = await context.SavedProperties
            .Where(sp => sp.UserId == userId && sp.Active)
            .Include(sp => sp.Property)
            .FirstOrDefaultAsync(sp => sp.Property.PropertyId == propertyId);
        if (savedProperty == null)
        {
            return false;
        }
        savedProperty.Active = false;
        await context.SaveChangesAsync();
        return true;
    }

    #endregion

    #region Property Management

    public async Task<PropertyResponseDto?> GetPropertyById(int propertyId)
    {
        using var context = Context.CreateDbContext();
        var property = await context.Properties
            .AsNoTracking()
            .Include(p => p.Media)
            .Where(p => p.PropertyId == propertyId)
            .FirstOrDefaultAsync();

        return property?.ToPropertyResponseDto();
    }

    public async Task<PagedResult<PropertyResponseDto>> SearchAsync(PropertySearchRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        using var context = Context.CreateDbContext();

        var query = context.Properties
            .AsNoTracking()
            .Where(p => p.IsActive)
            .AsQueryable();

        if (request.MinPrice.HasValue)
            query = query.Where(p => p.ListPrice >= request.MinPrice.Value);

        if (request.MaxPrice.HasValue)
            query = query.Where(p => p.ListPrice <= request.MaxPrice.Value);

        if (request.MinBedrooms.HasValue)
            query = query.Where(p => p.BedroomsTotal >= request.MinBedrooms.Value);

        if (request.MinBathrooms.HasValue)
            query = query.Where(p => p.BathroomsTotal >= request.MinBathrooms.Value);

        if (request.MinBuildingArea.HasValue)
            query = query.Where(p => p.BuildingAreaTotal >= request.MinBuildingArea.Value);

        if (!string.IsNullOrWhiteSpace(request.City))
            query = query.Where(p => p.City == request.City);

        if (!string.IsNullOrWhiteSpace(request.StateOrProvince))
            query = query.Where(p => p.StateOrProvince == request.StateOrProvince);

        if (!string.IsNullOrWhiteSpace(request.PostalCode))
            query = query.Where(p => p.PostalCode == request.PostalCode);

        if (!string.IsNullOrWhiteSpace(request.PropertyType))
            query = query.Where(p => p.PropertyType == request.PropertyType);

        if (!string.IsNullOrWhiteSpace(request.StandardStatus))
            query = query.Where(p => p.StandardStatus == request.StandardStatus);

        if (request.BrokerageOnly == true)
            query = query.Where(p => p.IsBrokerageListing);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();

            query = query.Where(p =>
                p.UnparsedAddress!.Contains(search) ||
                p.City!.Contains(search) ||
                p.PostalCode!.Contains(search));
        }

        var totalCount = await query.CountAsync();

        query = request.SortBy.ToLowerInvariant() switch
        {
            "listprice" => request.SortDescending
                ? query.OrderByDescending(p => p.ListPrice)
                : query.OrderBy(p => p.ListPrice),
            "bedroomstotal" => request.SortDescending
                ? query.OrderByDescending(p => p.BedroomsTotal)
                : query.OrderBy(p => p.BedroomsTotal),
            "buildingareatotal" => request.SortDescending
                ? query.OrderByDescending(p => p.BuildingAreaTotal)
                : query.OrderBy(p => p.BuildingAreaTotal),
            _ => request.SortDescending
                ? query.OrderByDescending(p => p.ModificationTimestamp)
                : query.OrderBy(p => p.ModificationTimestamp)
        };

        var items = await query
            .Include(p => p.Media)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var itemDtos = items.Select(p => p.ToPropertyResponseDto()).ToList();

        return new PagedResult<PropertyResponseDto>
        {
            Items = itemDtos,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    #endregion
}