namespace AxisUtah.Api.Services;

public interface IBrokerageService
{
    Task<BrokerageResponseDto?> GetBrokerageByIdAsync(int id);
}

public class BrokerageService(IDbContextFactory<AppDbContext> dbContextFactory) : IBrokerageService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory = dbContextFactory;

    public async Task<BrokerageResponseDto?> GetBrokerageByIdAsync(int id)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var brokerage = await dbContext.Brokerages.FindAsync(id);
        if (brokerage == null) return null;
        return new BrokerageResponseDto
        {
            Name = brokerage.Name,
            Address = brokerage.Address,
            City = brokerage.City,
            StateOrProvince = brokerage.StateOrProvince,
            PostalCode = brokerage.PostalCode,
            Phone = brokerage.Phone,
            Email = brokerage.Email,
            Website = brokerage.Website
        };
    }
}