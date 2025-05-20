using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.SupportAccountAggregate.Events;

public record SupportAccountActivatedEvent : IDomainEvent
{
    public Guid SupportAccountId { get; }
    public DateTime OccurredOn { get; }

    public SupportAccountActivatedEvent(
        Guid supportAccountId,
        DateTime occurredOn)
    {
        SupportAccountId = supportAccountId;
        OccurredOn = occurredOn;
    }
}