using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.ParamTypeAggregate.Events;

public record ParamTypeDisabledEvent(
    Guid ParamTypeId,
    DateTime OccurredOn) : IDomainEvent;