public record GetAllBanksRequest
{
    /// <summary>Page number (default = 1).</summary>
    public int? PageNumber { get; init; } = 1;

    /// <summary>Page size (default = 10).</summary>
    public int? PageSize { get; init; } = 10;

    /// <summary>Filter by bank code.</summary>
    public string? Code { get; init; }

    /// <summary>Filter by bank name.</summary>
    public string? Name { get; init; }

    /// <summary>Filter by abbreviation.</summary>
    public string? Abbreviation { get; init; }

    /// <summary>Status filter (Enabled/Disabled).</summary>
    public bool? IsEnabled { get; init; } = true;
}
