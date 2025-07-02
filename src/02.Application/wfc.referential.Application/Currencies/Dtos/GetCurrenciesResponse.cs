namespace wfc.referential.Application.Currencies.Dtos;

public record GetCurrenciesResponse
{
    /// <summary>
    /// Unique identifier of the currency.
    /// </summary>
    /// <example>c1a2b3d4-e5f6-7890-abcd-1234567890ef</example>
    public Guid CurrencyId { get; init; }

    /// <summary>
    /// Currency code.
    /// </summary>
    /// <example>USD</example>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// Currency code in Arabic.
    /// </summary>
    /// <example>دولار</example>
    public string CodeAR { get; init; } = string.Empty;

    /// <summary>
    /// Currency code in English.
    /// </summary>
    /// <example>Dollar</example>
    public string CodeEN { get; init; } = string.Empty;

    /// <summary>
    /// Name of the currency.
    /// </summary>
    /// <example>US Dollar</example>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// ISO numeric code of the currency.
    /// </summary>
    /// <example>840</example>
    public int CodeIso { get; init; }

    /// <summary>
    /// Indicates whether the currency is enabled.
    /// </summary>
    /// <example>true</example>
    public bool IsEnabled { get; init; }
}