using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.SupportAccountAggregate.Events;

public record SupportAccountDisabledEvent : IDomainEvent
{
    public Guid SupportAccountId { get; }
    public DateTime OccurredOn { get; }

    public SupportAccountDisabledEvent(
        Guid supportAccountId,
        DateTime occurredOn)
    {
        SupportAccountId = supportAccountId;
        OccurredOn = occurredOn;
    }
}