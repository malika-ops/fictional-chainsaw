namespace wfc.referential.Application.Currencies.Dtos;

public record PatchCurrencyRequest
{
    /// <summary>Unique currency code.</summary>
    public string? Code { get; init; }

    /// <summary>Display name.</summary>
    public string? Name { get; init; }

    /// <summary>Arabic code representation.</summary>
    public string? CodeAR { get; init; }

    /// <summary>English code representation.</summary>
    public string? CodeEN { get; init; }

    /// <summary>ISO 3-digit code.</summary>
    public int? CodeIso { get; init; }

    /// <summary>Currency status (enabled/disabled).</summary>
    public bool? IsEnabled { get; init; }
}
