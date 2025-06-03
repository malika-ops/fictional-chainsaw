using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.Countries;

namespace wfc.referential.Domain.CorridorAggregate.Events;

public record CorridorCreatedEvent(CorridorId id, CountryId? sourceCountry, CountryId? destCountry,
       CityId? sourceCity, CityId? destCity, AgencyId? sourceBranch, AgencyId? destBranch,
        bool IsEnabled)
    : IDomainEvent;