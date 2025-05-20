using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.PartnerAggregate.Events;

public record PartnerDisabledEvent : IDomainEvent
{
    public Guid PartnerId { get; }
    public DateTime OccurredOn { get; }

    public PartnerDisabledEvent(
        Guid partnerId,
        DateTime occurredOn)
    {
        PartnerId = partnerId;
        OccurredOn = occurredOn;
    }
}