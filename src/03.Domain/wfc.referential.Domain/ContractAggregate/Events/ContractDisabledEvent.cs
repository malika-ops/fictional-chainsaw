using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.ContractAggregate.Events;

public record ContractDisabledEvent(
    Guid ContractId,
    DateTime OccurredOn) : IDomainEvent;