namespace AxisUtah.Api.Services;

public interface IBrokerageService
{
    Task<BrokerageResponseDto?> GetBrokerageByIdAsync(int id);
    Task<BrokerageResponseDto?> UpdateBrokerageAsync(int id, BrokerageResponseDto brokerageDto);
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

    public async Task<BrokerageResponseDto?> UpdateBrokerageAsync(int id, BrokerageResponseDto brokerageDto)
    {
        if (id == 0) return null;
        
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var brokerage = await dbContext.Brokerages.FindAsync(id);
        if (brokerage == null) return null;

        brokerage.Name = brokerageDto.Name;
        brokerage.Address = brokerageDto.Address;
        brokerage.City = brokerageDto.City;
        brokerage.StateOrProvince = brokerageDto.StateOrProvince;
        brokerage.PostalCode = brokerageDto.PostalCode;
        brokerage.Phone = brokerageDto.Phone;
        brokerage.Email = brokerageDto.Email;
        brokerage.Website = brokerageDto.Website;

        await dbContext.SaveChangesAsync();

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