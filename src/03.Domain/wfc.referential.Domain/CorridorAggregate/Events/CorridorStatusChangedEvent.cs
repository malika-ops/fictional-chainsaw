using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.CorridorAggregate.Events;

public record CorridorStatusChangedEvent(Guid CorridorId, bool IsEnabled, DateTime ChangedAt) : IDomainEvent;
