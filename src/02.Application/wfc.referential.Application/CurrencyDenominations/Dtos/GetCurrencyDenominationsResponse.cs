using wfc.referential.Domain.CurrencyDenominationAggregate;

namespace wfc.referential.Application.CurrencyDenominations.Dtos;

public record GetCurrencyDenominationsResponse
{
    /// <summary>
    /// Unique identifier of the currency denomination.
    /// </summary>
    /// <example>c1a2b3d4-e5f6-7890-abcd-1234567890ef</example>
    public Guid CurrencyDenominationId { get; init; }

    /// <summary>
    /// Currency id.
    /// </summary>
    /// <example>c1a2b3d4-e5f6-7890-abcd-1234567890ef</example>
    public Guid CurrencyId { get; init; }

    /// <summary>
    /// Currency denomination type.
    /// </summary>
    /// <example>Coin</example>
    public CurrencyDenominationType Type { get; init; }

    /// <summary>
    /// Currency denomination value.
    /// </summary>
    /// <example>Dollar</example>
    public decimal Value { get; init; } 

    /// <summary>
    /// Indicates whether the currency denomination is enabled.
    /// </summary>
    /// <example>true</example>
    public bool IsEnabled { get; init; }
}