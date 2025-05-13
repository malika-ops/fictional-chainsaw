using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.Currencies.Dtos;

public record CreateCurrencyRequest
{
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
    ///<example>United States Dollar</example>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// A unique 3-digit number for the Currency.
    /// Must be unique.
    /// </summary>
    /// <example>840</example>
    public int CodeIso { get; init; }
}