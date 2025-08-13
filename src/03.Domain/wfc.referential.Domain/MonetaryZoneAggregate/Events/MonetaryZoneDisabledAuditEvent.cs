using BuildingBlocks.Core.Audit;

namespace wfc.referential.Domain.MonetaryZone.Events;

public record MonetaryZoneDisabledAuditEvent :AuditEntry
{
    public override string Service { get; init; } = "wfc.referential";
    public override string Action { get; init; } = "Disable";
    public override string Entity { get; init; } = "MonetaryZone";
}
