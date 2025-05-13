using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.CorridorAggregate.Events;
using wfc.referential.Domain.Countries;

namespace wfc.referential.Domain.CorridorAggregate;

public class Corridor : Aggregate<CorridorId>
{
    public CountryId SourceCountryId { get; private set; } = default!;
    public CountryId DestinationCountryId { get; private set; } = default!;
    public CityId? SourceCityId { get; private set; } = default!;
    public CityId? DestinationCityId { get; private set; } = default!;
    public AgencyId? SourceAgencyId { get; private set; } = default!;
    public AgencyId? DestinationAgencyId { get; private set; } = default!;
    public bool IsEnabled { get; private set; } = true;

    public Country? SourceCountry { get; private set; }
    public Country? DestinationCountry { get; private set; }
    public City? SourceCity { get; private set; }
    public City? DestinationCity { get; private set; }
    public Agency? SourceAgency { get; private set; }
    public Agency? DestinationAgency { get; private set; }
    public ICollection<TaxRuleDetailAggregate.TaxRuleDetail> TaxRuleDetails { get; private set; }

    private Corridor() { }

    public static Corridor Create(CorridorId id, CountryId sourceCountry, CountryId destCountry,
       CityId sourceCity, CityId destCity, AgencyId sourceAgency, AgencyId destAgency)
    {
        var corridor = new Corridor
        {
            Id = id,
            SourceCountryId = sourceCountry,
            DestinationCountryId = destCountry,
            SourceCityId = sourceCity,
            DestinationCityId = destCity,
            SourceAgencyId = sourceAgency,
            DestinationAgencyId = destAgency,
            IsEnabled = true
        };

        corridor.AddDomainEvent(new CorridorCreatedEvent(id.Value, sourceCountry, destCountry, sourceCity, destCity, sourceAgency, destAgency, true));
        return corridor;
    }

    public void SetInactive()
    {
        IsEnabled = false;
        AddDomainEvent(new CorridorStatusChangedEvent(Id!.Value, false, DateTime.UtcNow));
    }

    public void Update(CountryId sourceCountry, CountryId destCountry,
        CityId sourceCity, CityId destCity, AgencyId sourceAgency, AgencyId destAgency, bool isEnabled)
    {
        SourceCountryId = sourceCountry;
        DestinationCountryId = destCountry;
        SourceCityId = sourceCity;
        DestinationCityId = destCity;
        SourceAgencyId = sourceAgency;
        DestinationAgencyId = destAgency;
        IsEnabled = isEnabled;

        AddDomainEvent(new CorridorUpdatedEvent(
            Id!.Value, SourceCountryId, DestinationCountryId,
            SourceCityId, DestinationCityId,
            SourceAgencyId, DestinationAgencyId,
            isEnabled, DateTime.UtcNow
        ));
    }
    public void Patch(CountryId? sourceCountry = null, CountryId? destCountry = null,
       CityId? sourceCity = null, CityId? destCity = null,
       AgencyId? sourceAgency = null, AgencyId? destAgency = null,
       bool? isEnabled = null)
    {
        SourceCountryId = sourceCountry ?? SourceCountryId;
        DestinationCountryId = destCountry ?? DestinationCountryId;
        SourceCityId = sourceCity ?? SourceCityId;
        DestinationCityId = destCity ?? DestinationCityId;
        SourceAgencyId = sourceAgency ?? SourceAgencyId;
        DestinationAgencyId = destAgency ?? DestinationAgencyId;
        IsEnabled = isEnabled ?? IsEnabled;

        AddDomainEvent(new CorridorPatchedEvent(
            Id!.Value, SourceCountryId, DestinationCountryId,
            SourceCityId, DestinationCityId,
            SourceAgencyId, DestinationAgencyId,
            IsEnabled, DateTime.UtcNow
        ));
    }
}
