using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.ParamTypeAggregate.Events;

public record ParamTypeDisabledEvent : IDomainEvent
{
    public Guid ParamTypeId { get; }
    public DateTime OccurredOn { get; }

    public ParamTypeDisabledEvent(
        Guid paramTypeId,
        DateTime occurredOn)
    {
        ParamTypeId = paramTypeId;
        OccurredOn = occurredOn;
    }
}