using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.ContractAggregate.Events;

public record ContractActivatedEvent(
    Guid ContractId,
    DateTime OccurredOn) : IDomainEvent;