using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.ParamTypeAggregate.Events;

public record ParamTypePatchedEvent : IDomainEvent
{
    public Guid ParamTypeId { get; }
    public string Value { get; }
    public DateTime OccurredOn { get; }

    public ParamTypePatchedEvent(
        Guid paramTypeId,
        string value,
        DateTime occurredOn)
    {
        ParamTypeId = paramTypeId;
        Value = value;
        OccurredOn = occurredOn;
    }
}
