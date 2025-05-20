using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.Countries;

namespace wfc.referential.Domain.RegionAggregate.Events;

public record RegionCreatedEvent(Guid RegionId, string Code, string Name, bool IsEnabled, CountryId CountryId) : IDomainEvent;