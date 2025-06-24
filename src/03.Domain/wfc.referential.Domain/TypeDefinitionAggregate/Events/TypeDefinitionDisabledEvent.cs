using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.TypeDefinitionAggregate.Events;

public record TypeDefinitionDisabledEvent(
    Guid TypeDefinitionId,
    DateTime OccurredOn) : IDomainEvent;