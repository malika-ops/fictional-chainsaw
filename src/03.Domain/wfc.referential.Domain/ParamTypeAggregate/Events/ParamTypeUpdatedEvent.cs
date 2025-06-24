using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.ParamTypeAggregate.Events;

public record ParamTypeUpdatedEvent(
    Guid ParamTypeId,
    Guid TypeDefinitionId,
    string Value,
    bool IsEnabled,
    DateTime OccurredOn) : IDomainEvent;