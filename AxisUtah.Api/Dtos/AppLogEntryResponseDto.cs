namespace AxisUtah.Api.Dtos;

public record AppLogEntryResponseDto(
    long Id,
    DateTimeOffset CreatedAtUtc,
    string Level,
    string Category,
    string EventType,
    string Message,
    string Source,
    string? CorrelationId,
    string? DetailsJson
);