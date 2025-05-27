public record GetAllCurrenciesRequest
{
    /// <summary>Page number (default = 1).</summary>
    public int? PageNumber { get; init; } = 1;

    /// <summary>Page size (default = 10).</summary>
    public int? PageSize { get; init; } = 10;

    /// <summary>Filter by currency code.</summary>
    public string? Code { get; init; }

    /// <summary>Filter by Arabic code.</summary>
    public string? CodeAR { get; init; }

    /// <summary>Filter by English code.</summary>
    public string? CodeEN { get; init; }

    /// <summary>Filter by currency name.</summary>
    public string? Name { get; init; }

    /// <summary>Filter by ISO code.</summary>
    public int? CodeIso { get; init; }

    /// <summary>Status filter (Enabled/Disabled).</summary>
    public bool? IsEnabled { get; init; } = true;
}