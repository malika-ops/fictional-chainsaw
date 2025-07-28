using wfc.referential.Domain.CurrencyDenominationAggregate;

namespace wfc.referential.Application.CurrencyDenominations.Dtos;

public record UpdateCurrencyDenominationRequest
{
    /// <summary>Unique currency code.</summary>
    public Guid CurrencyId { get; init; }

    /// <summary>Display name.</summary>
    public CurrencyDenominationType Type { get; init; }

    /// <summary>Arabic code representation.</summary>
    public decimal Value { get; init; }

    /// <summary>Currency status (enabled/disabled).</summary>
    public bool IsEnabled { get; init; } = true;
}
