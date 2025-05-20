using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.CurrencyAggregate.Events;

public record CurrencyDisabledEvent : IDomainEvent
{
    public Guid CurrencyId { get; }
    public DateTime OccurredOn { get; }

    public CurrencyDisabledEvent(
        Guid currencyId,
        DateTime occurredOn)
    {
        CurrencyId = currencyId;
        OccurredOn = occurredOn;
    }
}