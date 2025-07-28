using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.CurrencyAggregate;

namespace wfc.referential.Domain.CurrencyDenominationAggregate.Events;

public record CurrencyDenominationPatchedEvent(
    Guid CurrencyDenominationId,
    CurrencyId CurrencyId,
    CurrencyDenominationType TypeCurrency,
    decimal Value,
    DateTime OccurredOn) : IDomainEvent;