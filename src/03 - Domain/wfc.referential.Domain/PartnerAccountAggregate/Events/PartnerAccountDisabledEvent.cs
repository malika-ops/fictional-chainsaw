using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.PartnerAccountAggregate.Events;

public record PartnerAccountDisabledEvent : IDomainEvent
{
    public Guid PartnerAccountId { get; }
    public DateTime OccurredOn { get; }

    public PartnerAccountDisabledEvent(
        Guid partnerAccountId,
        DateTime occurredOn)
    {
        PartnerAccountId = partnerAccountId;
        OccurredOn = occurredOn;
    }
}