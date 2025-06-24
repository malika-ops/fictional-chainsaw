using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.ParamTypeAggregate.Events;

public record ParamTypeActivatedEvent(
    Guid ParamTypeId,
    DateTime OccurredOn) : IDomainEvent;