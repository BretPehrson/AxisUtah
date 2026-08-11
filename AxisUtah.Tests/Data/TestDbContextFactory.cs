namespace AxisUtah.Tests.Data;

public static class TestDbContextFactory
{

    public static IDbContextFactory<AppDbContext> Create()
    {
        var baseConnectionString = GetConnectionStringFromAppsettings();

        // Strip the database name and use unique test DB name
        var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(baseConnectionString)
        {
            InitialCatalog = $"AxisUtahTests_{Guid.NewGuid():N}"
        };

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(builder.ConnectionString)
            .Options;

        using (var context = new AppDbContext(options))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }

        var factory = new PooledDbContextFactory<AppDbContext>(options);
        factory.CreateDbContext();
        return factory;
    }

    private static string? GetConnectionStringFromAppsettings()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();
        return config.GetConnectionString("DefaultConnection");
    }
}