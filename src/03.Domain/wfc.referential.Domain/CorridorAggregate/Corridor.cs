using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.CorridorAggregate.Events;
using wfc.referential.Domain.Countries;

namespace wfc.referential.Domain.CorridorAggregate;

public class Corridor : Aggregate<CorridorId>
{
    public CountryId? SourceCountryId { get; private set; } = default!;
    public CountryId? DestinationCountryId { get; private set; } = default!;
    public CityId? SourceCityId { get; private set; } = default!;
    public CityId? DestinationCityId { get; private set; } = default!;
    public AgencyId? SourceBranchId { get; private set; } = default!;
    public AgencyId? DestinationBranchId { get; private set; } = default!;
    public bool IsEnabled { get; private set; } = true;

    public Country? SourceCountry { get; private set; }
    public Country? DestinationCountry { get; private set; }
    public City? SourceCity { get; private set; }
    public City? DestinationCity { get; private set; }
    public Agency? SourceBranch { get; private set; }
    public Agency? DestinationBranch { get; private set; }
    public ICollection<TaxRuleDetailAggregate.TaxRuleDetail> TaxRuleDetails { get; private set; }

    private Corridor() { }

    public static Corridor Create(CorridorId id, CountryId? sourceCountry, CountryId? destCountry,
       CityId? sourceCity, CityId? destCity, AgencyId? sourceBranch, AgencyId? destBranch)
    {
        var corridor = new Corridor
        {
            Id = id,
            SourceCountryId = sourceCountry,
            DestinationCountryId = destCountry,
            SourceCityId = sourceCity,
            DestinationCityId = destCity,
            SourceBranchId = sourceBranch,
            DestinationBranchId = destBranch
        };

        corridor.AddDomainEvent(new CorridorCreatedEvent(id, sourceCountry, destCountry,
            sourceCity, destCity, sourceBranch, destBranch, true));
        return corridor;
    }

    public void SetInactive()
    {
        IsEnabled = false;
        AddDomainEvent(new CorridorStatusChangedEvent(Id!.Value, false, DateTime.UtcNow));
    }

    public void Update(Guid? sourceCountry, Guid? destCountry,
        Guid? sourceCity, Guid? destCity, Guid? sourceBranch, Guid? destBranch, bool isEnabled)
    {

        SourceCountryId = sourceCountry.HasValue ? CountryId.Of(sourceCountry.Value) : null;
        DestinationCountryId = destCountry.HasValue ? CountryId.Of(destCountry.Value) : null;
        SourceCityId = sourceCity.HasValue ? CityId.Of(sourceCity.Value) : null;
        DestinationCityId = destCity.HasValue ? CityId.Of(destCity.Value) : null;
        SourceBranchId = sourceBranch.HasValue ? AgencyId.Of(sourceBranch.Value) : null;
        DestinationBranchId = destBranch.HasValue ? AgencyId.Of(destBranch.Value) : null;
        IsEnabled = isEnabled;

        AddDomainEvent(new CorridorUpdatedEvent(
            Id!.Value, SourceCountryId, DestinationCountryId,
            SourceCityId, DestinationCityId,
            SourceBranchId, DestinationBranchId,
            isEnabled, DateTime.UtcNow
        ));
    }
    public void Patch(Guid? sourceCountryId, Guid? destinationCountryId, Guid? sourceCityId, Guid? destinationCityId, Guid? sourceBranchId, Guid? destinationBranchId, bool? isEnabled)
    {
        SourceCountryId = sourceCountryId.HasValue ? CountryId.Of(sourceCountryId.Value) : SourceCountryId;
        DestinationCountryId = destinationCountryId.HasValue ? CountryId.Of(destinationCountryId.Value) : DestinationCountryId;
        SourceCityId = sourceCityId.HasValue ? CityId.Of(sourceCityId.Value) : SourceCityId;
        DestinationCityId = destinationCityId.HasValue ? CityId.Of(destinationCityId.Value) : DestinationCityId;
        SourceBranchId = sourceBranchId.HasValue ? AgencyId.Of(sourceBranchId.Value) : SourceBranchId;
        DestinationBranchId = destinationBranchId.HasValue ? AgencyId.Of(destinationBranchId.Value) : DestinationBranchId;
        IsEnabled = isEnabled ?? IsEnabled;

        AddDomainEvent(new CorridorPatchedEvent(
            Id!.Value, SourceCountryId, DestinationCountryId,
            SourceCityId, DestinationCityId,
            SourceBranchId, DestinationBranchId,
            IsEnabled, DateTime.UtcNow
        ));
    }
}
