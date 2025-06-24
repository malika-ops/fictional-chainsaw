using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.TypeDefinitionAggregate.Events;

public record TypeDefinitionActivatedEvent(
    Guid TypeDefinitionId,
    DateTime OccurredOn) : IDomainEvent;