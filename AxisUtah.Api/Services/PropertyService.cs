namespace AxisUtah.Api.Services;

public class PropertyService(IDbContextFactory<AppDbContext> dbContextFactory)
{
    private IDbContextFactory<AppDbContext> Context { get; } = dbContextFactory;

    #region User Saved Properties

    public async Task<IEnumerable<PropertyResponseDto>> GetAllUserSavedProperties(int? userId)
    {
        using var context = Context.CreateDbContext();
        return await context.SavedProperties
            .Where(sp => sp.UserId == userId && sp.Active)
            .Select(sp => sp.Property)
            .Select(p => p.ToPropertyResponseDto())
            .ToListAsync();
    }

    public async Task<PropertyResponseDto?> GetUserSavedPropertyById(int? userId, int propertyId)
    {
        using var context = Context.CreateDbContext();
        var property = await context.SavedProperties
            .Where(sp => sp.UserId == userId && sp.Active)
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

        if (!string.IsNullOrWhiteSpace(request.City))
            query = query.Where(p => p.City == request.City);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();

            query = query.Where(p =>
                p.UnparsedAddress!.Contains(search) ||
                p.City!.Contains(search) ||
                p.PostalCode!.Contains(search));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(p => p.ModificationTimestamp)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        var itemDtos = items.Select(p => p.ToPropertyResponseDto()).ToList();

        return new PagedResult<PropertyResponseDto>
        {
            Items = itemDtos,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    #endregion
}