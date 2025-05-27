using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.CurrencyAggregate.Events;

public record CurrencyActivatedEvent(
    Guid CurrencyId,
    DateTime OccurredOn) : IDomainEvent;