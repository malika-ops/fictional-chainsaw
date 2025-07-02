using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.ContractDetailsAggregate.Events;

public record ContractDetailsDisabledEvent(
    Guid ContractDetailsId,
    DateTime OccurredOn) : IDomainEvent;