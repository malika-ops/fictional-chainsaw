using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.ContractDetailsAggregate.Events;

public record ContractDetailsCreatedEvent(
    Guid ContractDetailsId,
    Guid ContractId,
    Guid PricingId,
    bool IsEnabled,
    DateTime OccurredOn) : IDomainEvent;