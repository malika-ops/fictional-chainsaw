using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.TypeDefinitionAggregate.Events;

public record TypeDefinitionCreatedEvent(
    Guid TypeDefinitionId,
    string Libelle,
    string Description,
    bool IsEnabled,
    DateTime OccurredOn) : IDomainEvent;