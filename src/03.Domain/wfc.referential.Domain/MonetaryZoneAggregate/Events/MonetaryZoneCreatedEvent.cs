using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.MonetaryZoneAggregate;

namespace wfc.referential.Domain.MonetaryZone.Events;

public class MonetaryZoneCreatedEvent : IDomainEvent
{
    public Guid MonetaryZoneId { get; }
    public string Code { get; }
    public string Name { get; }
    public string Description { get; }
    public DateTime OccurredOn { get; }
    public bool IsEnabled { get; }

    public MonetaryZoneCreatedEvent(
        Guid monetaryZoneId,
        string code,
        string name,
        string description,
        bool isEnabled,
        DateTime occurredOn)
    {
        MonetaryZoneId = monetaryZoneId;
        Code = code;
        Name = name;
        Description = description;
        IsEnabled = isEnabled;
        OccurredOn = occurredOn;
    }
}