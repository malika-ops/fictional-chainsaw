using Newtonsoft.Json;

namespace wfc.referential.Domain.MonetaryZone.Events;

public record MonetaryZoneCreatedAuditEvent 
{
    public string UserId { get; init; }
    public string Service { get; init; } = "wfc.referential.api";
    public string Action { get; init; } = "Create";
    public string Entity { get; init; } = "MonetaryZone";
    public string EntityId { get; init; }
    public string? NewValueJson { get; init; }
    public string? OldValueJson { get; init; } = null;
    public string? MetadataJson { get; init; }
    public Guid TraceId { get; init; }
    public DateTime Timestamp { get; init; }

    public MonetaryZoneCreatedAuditEvent(
        string userId,
        Guid entityId,
        object newValue,
        object? metadata = null)
    {
        UserId = userId;
        EntityId = entityId.ToString();
        NewValueJson = JsonConvert.SerializeObject(newValue);
        MetadataJson = metadata != null ? JsonConvert.SerializeObject(metadata) : null;
        TraceId = ExtractTraceId(metadata);
        Timestamp = DateTime.UtcNow;
    }

    private static Guid ExtractTraceId(object? metadata)
    {
        if (metadata == null) return Guid.NewGuid();

        try
        {
            var json = JsonConvert.SerializeObject(metadata);
            var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            if (dict != null && dict.TryGetValue("TraceId", out var value)
                && Guid.TryParse(value?.ToString(), out var guid))
                return guid;
        }
        catch { }

        return Guid.NewGuid();
    }
}
