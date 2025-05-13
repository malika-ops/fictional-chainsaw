using BuildingBlocks.Core.Abstraction.Domain;


namespace wfc.referential.Domain.TierAggregate.Events;

public record TierPatchedEvent(
    Guid TierId,
    string Name,
    string Description,
    bool IsEnabled,
    DateTime OccurredOn) : IDomainEvent;