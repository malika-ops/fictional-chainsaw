using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.ContractDetailsAggregate.Events;

public record ContractDetailsPatchedEvent(
    Guid ContractDetailsId,
    Guid ContractId,
    Guid PricingId,
    bool IsEnabled,
    DateTime OccurredOn) : IDomainEvent;