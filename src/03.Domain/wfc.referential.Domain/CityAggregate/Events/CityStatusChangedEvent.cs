using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.CityAggregate.Events;

public record CityStatusChangedEvent(
    Guid RegionId, bool IsEnabled, DateTime OccurredOn) : IDomainEvent;