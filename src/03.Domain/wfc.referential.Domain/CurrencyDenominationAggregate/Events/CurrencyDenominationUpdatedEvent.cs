using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.CurrencyAggregate;

namespace wfc.referential.Domain.CurrencyDenominationAggregate.Events;

public record CurrencyDenominationUpdatedEvent(
    Guid CurrencyDenominationId,
    CurrencyId CurrencyId,
    CurrencyDenominationType TypeCurrencyDenomination,
    decimal Value,
    DateTime OccurredOn) : IDomainEvent;
