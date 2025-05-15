using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.PartnerCountryAggregate.Events;

public class PartnerCountryCreatedEvent : IDomainEvent
{
    public Guid PartnerCountryId { get; }
    public Guid PartnerId { get; }
    public Guid CountryId { get; }
    public bool IsEnabled { get; }
    public DateTime OccurredOn { get; }

    public PartnerCountryCreatedEvent(Guid partnerCountryId,
                                      Guid partnerId,
                                      Guid countryId,
                                      bool isEnabled,
                                      DateTime occurredOn)
    {
        PartnerCountryId = partnerCountryId;
        PartnerId = partnerId;
        CountryId = countryId;
        IsEnabled = isEnabled;
        OccurredOn = occurredOn;
    }
}
