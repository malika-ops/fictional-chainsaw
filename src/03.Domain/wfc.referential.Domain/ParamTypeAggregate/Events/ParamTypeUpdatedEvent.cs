using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.ParamTypeAggregate.Events;

public record ParamTypeUpdatedEvent : IDomainEvent
{
    public Guid ParamTypeId { get; }
    public string Value { get; }
    public DateTime OccurredOn { get; }

    public ParamTypeUpdatedEvent(
        Guid paramTypeId,
        string value)
    {
        ParamTypeId = paramTypeId;
        Value = value;
        OccurredOn = DateTime.UtcNow;
    }
}
