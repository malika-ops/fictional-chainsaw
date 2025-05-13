using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.TierAggregate.Events;

public record TierDisabledEvent(Guid TierId, DateTime OccurredOn) : IDomainEvent;
