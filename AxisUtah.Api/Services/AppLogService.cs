using System.Text.Json;

namespace AxisUtah.Api.Services;

public class AppLogService(AppDbContext db)
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    private readonly AppDbContext _db = db;

    public async Task WriteAsync(
        string level,
        string category,
        string eventType,
        string message,
        object? details = null,
        string? source = null,
        string? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        var logEntry = new AppLogEntry
        {
            Level = level,
            Category = category,
            EventType = eventType,
            Message = message,
            Source = source ?? "AxisUtah.Api",
            CorrelationId = correlationId,
            DetailsJson = SerializeDetails(details)
        };

        _db.AppLogEntries.Add(logEntry);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<AppLogEntryResponseDto>> QueryAsync(
        string? level = null,
        string? source = null,
        string? eventType = null,
        string? correlationId = null,
        string? search = null,
        DateTimeOffset? createdAfterUtc = null,
        DateTimeOffset? createdBeforeUtc = null,
        int skip = 0,
        int take = 100,
        CancellationToken cancellationToken = default)
    {
        var query = _db.AppLogEntries.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(level))
        {
            query = query.Where(entry => entry.Level == level);
        }

        if (!string.IsNullOrWhiteSpace(source))
        {
            query = query.Where(entry => entry.Source == source);
        }

        if (!string.IsNullOrWhiteSpace(eventType))
        {
            query = query.Where(entry => entry.EventType == eventType);
        }

        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            query = query.Where(entry => entry.CorrelationId == correlationId);
        }

        if (createdAfterUtc.HasValue)
        {
            query = query.Where(entry => entry.CreatedAtUtc >= createdAfterUtc.Value);
        }

        if (createdBeforeUtc.HasValue)
        {
            query = query.Where(entry => entry.CreatedAtUtc <= createdBeforeUtc.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(entry =>
                entry.Message.Contains(search) ||
                entry.Category.Contains(search) ||
                entry.EventType.Contains(search) ||
                (entry.CorrelationId != null && entry.CorrelationId.Contains(search)) ||
                (entry.DetailsJson != null && entry.DetailsJson.Contains(search)));
        }

        return await query
            .OrderByDescending(entry => entry.CreatedAtUtc)
            .Skip(Math.Max(skip, 0))
            .Take(Math.Clamp(take, 1, 500))
            .Select(entry => new AppLogEntryResponseDto(
                entry.Id,
                entry.CreatedAtUtc,
                entry.Level,
                entry.Category,
                entry.EventType,
                entry.Message,
                entry.Source,
                entry.CorrelationId,
                entry.DetailsJson))
            .ToListAsync(cancellationToken);
    }

    private static string? SerializeDetails(object? details)
    {
        if (details == null)
        {
            return null;
        }

        return details switch
        {
            JsonElement jsonElement => jsonElement.GetRawText(),
            _ => JsonSerializer.Serialize(details, SerializerOptions)
        };
    }
}