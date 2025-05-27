using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.BankAggregate.Events;

public record BankDisabledEvent(
    Guid BankId,
    DateTime OccurredOn) : IDomainEvent;