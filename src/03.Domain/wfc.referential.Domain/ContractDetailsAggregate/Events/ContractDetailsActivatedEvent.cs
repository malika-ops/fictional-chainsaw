using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.ContractDetailsAggregate.Events;

public record ContractDetailsActivatedEvent(
    Guid ContractDetailsId,
    DateTime OccurredOn) : IDomainEvent;