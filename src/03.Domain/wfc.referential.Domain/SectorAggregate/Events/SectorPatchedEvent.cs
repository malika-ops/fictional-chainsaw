using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.SectorAggregate.Events;

public record SectorPatchedEvent(
    Guid SectorId,
    string Code,
    string Name,
    Guid CityId,
    DateTime OccurredOn) : IDomainEvent;