using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.Currencies.Dtos;

public record UpdateCurrencyRequest
{
    /// <summary>
    /// The ID of the Currency to update.
    /// </summary>
    /// <example>6a472a58-5d05-4a1b-8b7f-58516dd614c3</example>
    public Guid CurrencyId { get; init; }

    /// <summary>
    /// A short code for the Currency.
    /// Must be unique.
    /// </summary>
    /// <example>USD</example>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// Arabic code representation for the Currency.
    /// </summary>
    /// <example>دولار</example>
    public string CodeAR { get; init; } = string.Empty;

    /// <summary>
    /// English code representation for the Currency.
    /// </summary>
    /// <example>Dollar</example>
    public string CodeEN { get; init; } = string.Empty;

    /// <summary>
    /// A human-readable name.
    /// </summary>
    /// <example>United States Dollar</example>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// A unique 3-digit number for the Currency.
    /// Must be unique.
    /// </summary>
    /// <example>840</example>
    public int CodeIso { get; init; }

    /// <summary>
    /// Whether the currency is enabled or disabled
    /// </summary>
    /// <example>true</example>
    public bool IsEnabled { get; init; } = true;
}