using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.Countries.Events;

public class CountryPatchedEvent : IDomainEvent
{
    public Guid CountryId { get; }
    public string Abbreviation { get; }
    public string Name { get; }
    public string Code { get; }
    public string ISO2 { get; }
    public string ISO3 { get; }
    public string DialingCode { get; }
    public string TimeZone { get; }
    public bool IsEnabled { get; }
    public Guid MonetaryZoneId { get; }
    public Guid? CurrencyId { get; }
    public DateTime OccurredOn { get; }

    public CountryPatchedEvent(Guid countryId,
                               string abbreviation,
                               string name,
                               string code,
                               string iso2,
                               string iso3,
                               string dialingCode,
                               string timeZone,
                               bool isEnabled,
                               Guid monetaryZoneId,
                               Guid? currencyId,
                               DateTime occurredOn)
    {
        CountryId = countryId;
        Abbreviation = abbreviation;
        Name = name;
        Code = code;
        ISO2 = iso2;
        ISO3 = iso3;
        DialingCode = dialingCode;
        TimeZone = timeZone;
        IsEnabled = isEnabled;
        MonetaryZoneId = monetaryZoneId;
        CurrencyId = currencyId;
        OccurredOn = occurredOn;
    }
}
