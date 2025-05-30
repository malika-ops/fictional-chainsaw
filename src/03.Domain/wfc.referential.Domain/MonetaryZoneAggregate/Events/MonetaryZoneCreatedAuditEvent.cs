using BuildingBlocks.Core.Audit.Interface;

namespace wfc.referential.Domain.MonetaryZone.Events;

public record MonetaryZoneCreatedAuditEvent(string UserId, Guid MonetaryZoneId,object newValue,object? metaData = null) 
    : IAuditDomainEvent
{
    public string Service => "Referential";

    public string Action => "Create";

    public string Entity => "MonetaryZone";

    public string EntityId => MonetaryZoneId.ToString();

    public object? NewValue => newValue;

    public object? OldValue => null;

    public object? Metadata => metaData;

}