using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.ParamTypeAggregate.Events;

public record ParamTypeActivatedEvent : IDomainEvent
{
    public Guid ParamTypeId { get; }
    public DateTime OccurredOn { get; }

    public ParamTypeActivatedEvent(
        Guid paramTypeId,
        DateTime occurredOn)
    {
        ParamTypeId = paramTypeId;
        OccurredOn = occurredOn;
    }
}