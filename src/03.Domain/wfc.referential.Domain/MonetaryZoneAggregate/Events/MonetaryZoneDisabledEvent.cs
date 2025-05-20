using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.MonetaryZoneAggregate.Events;

public class MonetaryZoneDisabledEvent : IDomainEvent
{
    public Guid MonetaryZoneId { get; }
    public DateTime OccurredOn { get; }

    public MonetaryZoneDisabledEvent(
        Guid monetaryZoneId,
        DateTime occurredOn)
    {
        MonetaryZoneId = monetaryZoneId;
        OccurredOn = occurredOn;
    }
}

