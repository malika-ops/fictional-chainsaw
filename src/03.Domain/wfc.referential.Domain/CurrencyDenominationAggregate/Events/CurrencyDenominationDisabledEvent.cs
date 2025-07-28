using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.CurrencyDenominationAggregate.Events;

public record CurrencyDenominationDisabledEvent(
    Guid CurrencyDenominationId,
    DateTime OccurredOn) : IDomainEvent;