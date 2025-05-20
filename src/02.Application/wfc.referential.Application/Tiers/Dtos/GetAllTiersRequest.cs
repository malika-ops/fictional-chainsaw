namespace wfc.referential.Application.Tiers.Dtos;

public record GetAllTiersRequest
{
    /// <summary>Page number (default = 1).</summary>
    public int? PageNumber { get; init; } = 1;

    /// <summary>Page size  (default = 10).</summary>
    public int? PageSize { get; init; } = 10;

    /// <summary>Optional filter by name.</summary>
    public string? Name { get; init; }

    /// <summary>Optional filter by description.</summary>
    public string? Description { get; init; }

    /// <summary>Optional filter by enabled flag.</summary>
    public bool? IsEnabled { get; init; } = true;
}
