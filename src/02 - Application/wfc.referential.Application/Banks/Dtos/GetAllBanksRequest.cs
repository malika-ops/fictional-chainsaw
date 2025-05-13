namespace wfc.referential.Application.Banks.Dtos;

public record GetAllBanksRequest
{
    /// <summary>Optional page number (default = 1).</summary>
    public int? PageNumber { get; init; } = 1;

    /// <summary>Optional page size (default = 10).</summary>
    public int? PageSize { get; init; } = 10;

    /// <summary>Optional filter by code.</summary>
    public string? Code { get; init; }

    /// <summary>Optional filter by name.</summary>
    public string? Name { get; init; }

    /// <summary>Optional filter by abbreviation.</summary>
    public string? Abbreviation { get; init; }

    /// <summary>Optional filter by IsEnabled status.</summary>
    public bool? IsEnabled { get; init; } = true;
}