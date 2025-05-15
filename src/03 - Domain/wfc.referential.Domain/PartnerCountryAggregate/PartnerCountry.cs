using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.PartnerCountryAggregate.Events;

namespace wfc.referential.Domain.PartnerCountryAggregate;

public class PartnerCountry : Aggregate<PartnerCountryId>
{
    public PartnerId PartnerId { get; private set; }
    public CountryId CountryId { get; private set; }
    public bool IsEnabled { get; private set; } = true;
    public Partner? Partner { get; private set; }
    public Country? Country { get; private set; }

    private PartnerCountry() { }

    public static PartnerCountry Create(PartnerCountryId id,
                                    PartnerId partnerId,
                                    CountryId countryId,
                                    bool isEnabled = true)
    {
        var entity = new PartnerCountry
        {
            Id = id,
            PartnerId = partnerId,
            CountryId = countryId,
            IsEnabled = isEnabled
        };

        entity.AddDomainEvent(new PartnerCountryCreatedEvent(
            entity.Id.Value,
            entity.PartnerId.Value,
            entity.CountryId.Value,
            entity.IsEnabled,
            DateTime.UtcNow));

        return entity;
    }

    public void Update(PartnerId partnerId,
                   CountryId countryId,
                   bool isEnabled)
    {
        PartnerId = partnerId;
        CountryId = countryId;
        IsEnabled = isEnabled;

        AddDomainEvent(new PartnerCountryUpdatedEvent(
            Id.Value, PartnerId.Value, CountryId.Value, IsEnabled, DateTime.UtcNow));
    }

    public void Disable()
    {
        IsEnabled = false;

        AddDomainEvent(new PartnerCountryDisabledEvent(
            Id.Value,
            DateTime.UtcNow));
    }
}