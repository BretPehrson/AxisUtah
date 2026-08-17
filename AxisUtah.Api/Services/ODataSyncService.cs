namespace AxisUtah.Api.Services;

public class ODataSyncService(HttpClient httpClient, AppDbContext db, AppLogService appLogService, ILogger<ODataSyncService> logger)
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly AppDbContext _db = db;
    private readonly AppLogService _appLogService = appLogService;
    private readonly ILogger<ODataSyncService> _logger = logger;

    public async Task<DateTimeOffset?> SyncListingsAsync(DateTimeOffset lastSync, string? correlationId = null, CancellationToken cancellationToken = default)
    {
        var startedAt = DateTimeOffset.UtcNow;
        var pageCount = 0;
        var insertedCount = 0;
        var updatedCount = 0;

        string oDataSelect = "$select=ListingKey,ListingId,ListPrice,BedroomsTotal,BathroomsTotalInteger,BuildingAreaTotal,StandardStatus,PropertyType,StructureType,PublicRemarks,UnparsedAddress,City,StateOrProvince,PostalCode,Latitude,Longitude,ListAgentFullName,ListOfficeName,ModificationTimestamp,Media";
        string oDataFilter = $"$filter=ModificationTimestamp gt {lastSync:yyyy-MM-ddTHH:mm:ssZ} and StandardStatus eq 'Active'";
        string? requestUrl = $"Property?{oDataSelect}&{oDataFilter}";
        DateTimeOffset? latestModificationTimestamp = null;

        while (!string.IsNullOrWhiteSpace(requestUrl))
        {
            pageCount++;
            _logger.LogDebug("Fetching MLS property page {PageNumber} from {RequestUrl}.", pageCount, requestUrl);

            var response = await _httpClient.GetFromJsonAsync<ResoODataResponse>(requestUrl, cancellationToken);

            if (response?.Value == null || response.Value.Count == 0)
            {
                _logger.LogInformation("MLS property page {PageNumber} returned no records.", pageCount);
                break;
            }

            foreach (var dto in response.Value)
            {
                if (!latestModificationTimestamp.HasValue || dto.ModificationTimestamp > latestModificationTimestamp.Value)
                {
                    latestModificationTimestamp = dto.ModificationTimestamp;
                }

                var existingProperty = await _db.Properties
                    .Include(p => p.Media)
                    .FirstOrDefaultAsync(p => p.ListingKey == dto.ListingKey, cancellationToken);

                if (existingProperty == null)
                {
                    var newProperty = dto.ToEntity();
                    _db.Properties.Add(newProperty);
                    insertedCount++;
                }
                else
                {
                    existingProperty.UpdateEntityFromDto(dto);
                    updatedCount++;
                }
            }

            await _db.SaveChangesAsync(cancellationToken);
            requestUrl = response.NextLink;
        }

        var durationMs = (DateTimeOffset.UtcNow - startedAt).TotalMilliseconds;
        await _appLogService.WriteAsync(
            level: "Information",
            category: nameof(ODataSyncService),
            eventType: "PropertySyncSummary",
            message: "MLS property sync completed.",
            details: new
            {
                pageCount,
                insertedCount,
                updatedCount,
                lastSync,
                latestModificationTimestamp,
                durationMs
            },
            source: "AxisUtah.Api",
            correlationId: correlationId,
            cancellationToken: cancellationToken);

        return latestModificationTimestamp;
    }
}