using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.OperatorAggregate.Events;

public record OperatorDisabledEvent(
    Guid OperatorId,
    DateTime OccurredOn) : IDomainEvent;