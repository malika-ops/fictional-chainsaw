using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.Countries;

namespace wfc.referential.Domain.CorridorAggregate.Events;

public record CorridorCreatedEvent(Guid CorridorId, CountryId SourceCountryId, CountryId DestinationCountryId,
        CityId SourceCityId, CityId DestinationCityId, AgencyId SourceAgencyId, AgencyId DestinationAgencyId,
        bool IsEnabled)
    : IDomainEvent;