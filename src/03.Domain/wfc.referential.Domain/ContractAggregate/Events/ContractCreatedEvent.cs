using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.ContractAggregate.Events;

public record ContractCreatedEvent(
    Guid ContractId,
    string Code,
    Guid PartnerId,
    DateTime StartDate,
    DateTime EndDate,
    bool IsEnabled,
    DateTime OccurredOn) : IDomainEvent;