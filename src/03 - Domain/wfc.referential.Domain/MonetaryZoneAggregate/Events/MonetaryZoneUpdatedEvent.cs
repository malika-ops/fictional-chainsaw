using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.MonetaryZone.Events;

public class MonetaryZoneUpdatedEvent : IDomainEvent
{
    public Guid MonetaryZoneId { get; }
    public string Code { get; } = string.Empty;
    public string Name { get; } = string.Empty;
    public string Description { get; } = string.Empty;
    public DateTime OccurredOn { get; }

    public MonetaryZoneUpdatedEvent(
        Guid monetaryZoneId,
        string code,
        string name,
        string description,
        DateTime occurredOn)
    {
        MonetaryZoneId = monetaryZoneId;
        Code = code;
        Name = name;
        Description = description;
        OccurredOn = occurredOn;
    }
}
