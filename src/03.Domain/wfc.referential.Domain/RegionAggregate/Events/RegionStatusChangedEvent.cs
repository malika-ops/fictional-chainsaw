using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.RegionAggregate.Events;

public record RegionStatusChangedEvent(Guid RegionId, bool IsEnabled, DateTime OccurredOn) : IDomainEvent;