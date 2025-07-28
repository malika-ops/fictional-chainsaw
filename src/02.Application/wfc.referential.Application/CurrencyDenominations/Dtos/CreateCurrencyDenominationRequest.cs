using wfc.referential.Domain.CurrencyDenominationAggregate;

namespace wfc.referential.Application.CurrencyDenominations.Dtos;

public record CreateCurrencyDenominationRequest
{
    /// <summary>currency id.</summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid CurrencyId { get; init; }

    /// <summary>Currency Denomination Type.</summary>
    /// <example>Coin / Banknote</example>
    public CurrencyDenominationType Type { get; init; }

    /// <summary>Value.</summary>
    /// <example>2</example>
    public decimal Value { get; init; }
}