using System.Text.Json;

namespace AxisUtah.Api.Dtos;

public record LogEntryDto(
    string Level,
    string Category,
    string EventType,
    string Message,
    string Source,
    string? CorrelationId,
    JsonElement? Details
);