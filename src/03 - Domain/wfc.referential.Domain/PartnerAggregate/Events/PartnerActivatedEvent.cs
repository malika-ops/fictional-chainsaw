using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.PartnerAggregate.Events;

public record PartnerActivatedEvent : IDomainEvent
{
    public Guid PartnerId { get; }
    public DateTime OccurredOn { get; }

    public PartnerActivatedEvent(
        Guid partnerId,
        DateTime occurredOn)
    {
        PartnerId = partnerId;
        OccurredOn = occurredOn;
    }
}