using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.TierAggregate.Events;

public record TierCreatedEvent(
    Guid TierId,
    string Name,
    DateTime OccurredOn) : IDomainEvent;