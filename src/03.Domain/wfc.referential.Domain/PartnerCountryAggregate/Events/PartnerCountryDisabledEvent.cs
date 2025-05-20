using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.PartnerCountryAggregate.Events;

public class PartnerCountryDisabledEvent : IDomainEvent
{
    public Guid PartnerCountryId { get; }
    public DateTime OccurredOn { get; }

    public PartnerCountryDisabledEvent(Guid partnerCountryId, DateTime occurredOn)
    {
        PartnerCountryId = partnerCountryId;
        OccurredOn = occurredOn;
    }
}