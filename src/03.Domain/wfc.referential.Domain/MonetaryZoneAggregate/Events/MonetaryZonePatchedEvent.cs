using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.MonetaryZone.Events;

public class MonetaryZonePatchedEvent : IDomainEvent
{
    public Guid MonetaryZoneId { get; }
    public string Code { get; }
    public string Name { get; }
    public string Description { get; }
    public DateTime OccurredOn { get; }

    public MonetaryZonePatchedEvent(
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
