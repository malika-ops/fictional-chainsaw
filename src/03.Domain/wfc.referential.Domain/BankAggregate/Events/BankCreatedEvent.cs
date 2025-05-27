using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.BankAggregate.Events;

public record BankCreatedEvent(
    Guid BankId,
    string Code,
    string Name,
    string Abbreviation,
    bool IsEnabled,
    DateTime OccurredOn) : IDomainEvent;