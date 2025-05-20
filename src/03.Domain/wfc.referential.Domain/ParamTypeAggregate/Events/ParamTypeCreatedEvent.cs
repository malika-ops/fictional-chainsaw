using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.ParamTypeAggregate.Events;

public record ParamTypeCreatedEvent : IDomainEvent
{
    public Guid ParamTypeId { get; }
    public string Value { get; }
    public bool IsEnabled { get; }
    public DateTime OccurredOn { get; }

    public ParamTypeCreatedEvent(
        Guid paramTypeId,
        string value,
        bool isEnabled,
        DateTime occurredOn)
    {
        ParamTypeId = paramTypeId;
        Value = value;
        IsEnabled = isEnabled;
        OccurredOn = occurredOn;
    }
}