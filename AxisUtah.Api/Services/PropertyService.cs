namespace AxisUtah.Api.Services;

public class PropertyService(IDbContextFactory<AppDbContext> dbContextFactory)
{
    private IDbContextFactory<AppDbContext> Context { get; } = dbContextFactory;

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
}