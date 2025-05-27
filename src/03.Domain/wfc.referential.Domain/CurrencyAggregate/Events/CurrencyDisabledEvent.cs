using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.CurrencyAggregate.Events;

public record CurrencyDisabledEvent(
    Guid CurrencyId,
    DateTime OccurredOn) : IDomainEvent;