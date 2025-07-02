namespace wfc.referential.Application.Currencies.Dtos;

public record UpdateCurrencyRequest
{
    /// <summary>Unique currency code.</summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>Display name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Arabic code representation.</summary>
    public string CodeAR { get; init; } = string.Empty;

    /// <summary>English code representation.</summary>
    public string CodeEN { get; init; } = string.Empty;

    /// <summary>ISO 3-digit code.</summary>
    public int CodeIso { get; init; }

    /// <summary>Currency status (enabled/disabled).</summary>
    public bool IsEnabled { get; init; } = true;
}
