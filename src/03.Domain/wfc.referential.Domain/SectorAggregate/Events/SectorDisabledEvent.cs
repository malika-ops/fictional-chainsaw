using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.SectorAggregate.Events;

public record SectorDisabledEvent(
    Guid SectorId,
    DateTime OccurredOn) : IDomainEvent;