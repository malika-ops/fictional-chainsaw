using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.BankAggregate.Events;

public record BankPatchedEvent(
    Guid BankId,
    string Code,
    string Name,
    string Abbreviation,
    bool IsEnabled,
    DateTime OccurredOn) : IDomainEvent;