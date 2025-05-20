using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.PartnerAccountAggregate.Events;

public record PartnerAccountActivatedEvent : IDomainEvent
{
    public Guid PartnerAccountId { get; }
    public DateTime OccurredOn { get; }

    public PartnerAccountActivatedEvent(
        Guid partnerAccountId,
        DateTime occurredOn)
    {
        PartnerAccountId = partnerAccountId;
        OccurredOn = occurredOn;
    }
}