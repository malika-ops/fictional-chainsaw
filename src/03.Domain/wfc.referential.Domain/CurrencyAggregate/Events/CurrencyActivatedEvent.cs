using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.CurrencyAggregate.Events;

public record CurrencyActivatedEvent : IDomainEvent
{
    public Guid CurrencyId { get; }
    public DateTime OccurredOn { get; }

    public CurrencyActivatedEvent(
        Guid currencyId,
        DateTime occurredOn)
    {
        CurrencyId = currencyId;
        OccurredOn = occurredOn;
    }
}