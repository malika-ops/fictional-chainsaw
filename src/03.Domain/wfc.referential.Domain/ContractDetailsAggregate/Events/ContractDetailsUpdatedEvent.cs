using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.ContractDetailsAggregate.Events;

public record ContractDetailsUpdatedEvent(
    Guid ContractDetailsId,
    Guid ContractId,
    Guid PricingId,
    bool IsEnabled,
    DateTime OccurredOn) : IDomainEvent;