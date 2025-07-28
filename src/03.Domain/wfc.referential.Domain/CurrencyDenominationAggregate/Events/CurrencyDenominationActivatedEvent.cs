using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.CurrencyDenominationAggregate.Events;

public record CurrencyDenominationActivatedEvent(
    Guid CurrencyDenominationId,
    DateTime OccurredOn) : IDomainEvent;