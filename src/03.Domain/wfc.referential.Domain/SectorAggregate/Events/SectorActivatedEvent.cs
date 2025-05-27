using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.SectorAggregate.Events;

public record SectorActivatedEvent(
    Guid SectorId,
    DateTime OccurredOn) : IDomainEvent;