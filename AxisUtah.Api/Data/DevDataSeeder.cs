namespace AxisUtah.Api.Data;

// Seeds fake property data for local testing when the MLS bearer token isn't available.
public static class DevDataSeeder
{
    public static async Task SeedTestPropertiesAsync(AppDbContext db, CancellationToken cancellationToken = default)
    {
        if (await db.Properties.IgnoreQueryFilters().AnyAsync(cancellationToken))
        {
            return;
        }

        var brokerage = new Brokerage
        {
            Name = "Wasatch Realty Group",
            Address = "123 Main St",
            City = "Salt Lake City",
            StateOrProvince = "UT",
            PostalCode = "84101",
            Phone = "801-555-0100",
            Email = "info@wasatchrealty.test",
            Website = "https://wasatchrealty.test"
        };

        var agent = new Agent
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane.doe@wasatchrealty.test",
            Phone = "801-555-0101",
            Brokerage = brokerage
        };

        db.Brokerages.Add(brokerage);
        db.Agents.Add(agent);

        var random = new Random(42);
        var cities = new[] { "Salt Lake City", "Provo", "Ogden", "Park City", "St. George" };
        var propertyTypes = new[] { "Residential", "Condominium", "Townhouse" };
        var structureTypes = new[] { "House", "Multi Family", "Twin Home" };

        var properties = new List<Property>();

        for (var i = 1; i <= 25; i++)
        {
            var city = cities[i % cities.Length];

            var address = new Address
            {
                UnparsedAddress = $"{100 + i} Test Ave",
                City = city,
                StateOrProvince = "UT",
                PostalCode = $"840{i:00}",
                Latitude = 40.7608 + i * 0.01,
                Longitude = -111.8910 - i * 0.01
            };

            var property = new Property
            {
                ListingKey = 1000000 + i,
                ListingId = $"TEST-{i:0000}",
                ListPrice = 300_000 + random.Next(0, 700_000),
                BedroomsTotal = random.Next(2, 6),
                BathroomsTotal = random.Next(1, 4),
                BuildingAreaTotal = random.Next(1200, 4500),
                StandardStatus = "Active",
                PropertyType = propertyTypes[i % propertyTypes.Length],
                StructureType = structureTypes[i % structureTypes.Length],
                PublicRemarks = $"Test listing #{i} generated for local development.",
                UnparsedAddress = address.UnparsedAddress,
                City = address.City,
                StateOrProvince = address.StateOrProvince,
                PostalCode = address.PostalCode,
                Latitude = address.Latitude,
                Longitude = address.Longitude,
                ListAgentFullName = $"{agent.FirstName} {agent.LastName}",
                ListOfficeName = brokerage.Name,
                IsBrokerageListing = true,
                IsActive = true,
                Address = address,
                Agent = agent,
                Brokerage = brokerage,
                ModificationTimestamp = DateTimeOffset.UtcNow.AddDays(-i),
                LastSyncedAt = DateTimeOffset.UtcNow,
                Media =
                [
                    new PropertyMedia { MediaUrl = $"https://picsum.photos/seed/{i}-1/800/600", Order = 0 },
                    new PropertyMedia { MediaUrl = $"https://picsum.photos/seed/{i}-2/800/600", Order = 1 }
                ]
            };

            properties.Add(property);
        }

        db.Properties.AddRange(properties);

        await db.SaveChangesAsync(cancellationToken);
    }
}
