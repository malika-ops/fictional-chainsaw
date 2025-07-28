using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.CurrencyAggregate;
using wfc.referential.Domain.CurrencyDenominationAggregate.Events;

namespace wfc.referential.Domain.CurrencyDenominationAggregate;

public class CurrencyDenomination : Aggregate<CurrencyDenominationId>
{
    public CurrencyId CurrencyId { get; set; }  
    public CurrencyDenominationType Type { get; set; }
    public Decimal Value { get; set; }
    public bool IsEnabled { get; private set; } = true;

    private CurrencyDenomination() { }

    public static CurrencyDenomination Create(
        CurrencyDenominationId id,
        CurrencyId currencyId,
        CurrencyDenominationType typeCurrency,
        decimal value
        )
    {
        var CurrencyDenomination = new CurrencyDenomination
        {
            Id = id,
            CurrencyId = currencyId,
            Type = typeCurrency,
            Value = value,
            IsEnabled = true
        };

        CurrencyDenomination.AddDomainEvent(new CurrencyDenominationCreatedEvent(
            CurrencyDenomination.Id.Value,
            CurrencyDenomination.CurrencyId,
            CurrencyDenomination.Type,
            CurrencyDenomination.Value,
            DateTime.UtcNow));

        return CurrencyDenomination;
    }

    public void Update(
        CurrencyId currencyId,
        CurrencyDenominationType typeCurrency,
        decimal value,
        bool? isEnabled)
    {
        CurrencyId = currencyId;
        Type = typeCurrency;
        Value = value;
        IsEnabled = isEnabled ?? IsEnabled;

        AddDomainEvent(new CurrencyDenominationUpdatedEvent(
            Id.Value,
            CurrencyId,
            Type,
            Value,
            DateTime.UtcNow));
    }

    public void Patch(
        CurrencyId? currencyId,
        CurrencyDenominationType? typeCurrency,
        decimal? value,
        bool? isEnabled)
    {
        CurrencyId = currencyId ?? CurrencyId;
        Type = typeCurrency ?? Type;
        Value = value ?? Value;
        IsEnabled = isEnabled ?? IsEnabled;

        AddDomainEvent(new CurrencyDenominationPatchedEvent(
            Id.Value,
            CurrencyId,
            Type,
            Value,
            DateTime.UtcNow));
    }

    public void Disable()
    {
        IsEnabled = false;

        AddDomainEvent(new CurrencyDenominationDisabledEvent(
            Id.Value,
            DateTime.UtcNow));
    }

    public void Activate()
    {
        IsEnabled = true;

        AddDomainEvent(new CurrencyDenominationActivatedEvent(
            Id.Value,
            DateTime.UtcNow));
    }
}