using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.CurrencyAggregate.Events;

public record CurrencyPatchedEvent(
    Guid CurrencyId,
    string Code,
    string CodeAR,
    string CodeEN,
    string Name,
    int CodeIso,
    DateTime OccurredOn) : IDomainEvent;