namespace wfc.referential.Application.Currencies.Dtos;

public record CreateCurrencyRequest
{
    /// <summary>Unique currency code.</summary>
    /// <example>USD</example>
    public string Code { get; init; } = string.Empty;

    /// <summary>Currency Name.</summary>
    /// <example>United States Dollar</example>
    public string Name { get; init; } = string.Empty;

    /// <summary>Arabic code representation.</summary>
    /// <example>دولار</example>
    public string CodeAR { get; init; } = string.Empty;

    /// <summary>English code representation.</summary>
    /// <example>Dollar</example>
    public string CodeEN { get; init; } = string.Empty;

    /// <summary>ISO 3-digit code.</summary>
    /// <example>840</example>
    public int CodeIso { get; init; }
}