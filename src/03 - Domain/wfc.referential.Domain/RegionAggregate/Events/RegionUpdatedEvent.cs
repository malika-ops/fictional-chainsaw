using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.Countries;

namespace wfc.referential.Domain.RegionAggregate.Events;

public record RegionUpdatedEvent(Guid RegionId, string Code, string Name, bool IsEnabled,
    CountryId CountryId, DateTime OccurredOn) : IDomainEvent;