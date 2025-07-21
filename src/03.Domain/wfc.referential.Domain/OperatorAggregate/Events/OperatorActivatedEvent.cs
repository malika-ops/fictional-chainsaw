using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.OperatorAggregate.Events;

public record OperatorActivatedEvent(
    Guid OperatorId,
    DateTime OccurredOn) : IDomainEvent;