using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.BankAggregate.Events;

public record BankActivatedEvent(
    Guid BankId,
    DateTime OccurredOn) : IDomainEvent;
