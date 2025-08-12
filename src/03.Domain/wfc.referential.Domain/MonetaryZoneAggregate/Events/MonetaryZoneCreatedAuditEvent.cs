using BuildingBlocks.Core.Audit;

namespace wfc.referential.Domain.MonetaryZone.Events;


public record MonetaryZoneCreatedAuditEvent :AuditEntry
{
    public override string Service { get; init; } = "wfc.referential";
    public override string Action { get; init; } = "Create";
    public override string Entity { get; init; } = "MonetaryZone";
}
