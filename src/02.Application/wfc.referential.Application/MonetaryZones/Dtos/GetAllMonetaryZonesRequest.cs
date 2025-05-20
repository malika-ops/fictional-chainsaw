namespace wfc.referential.Application.MonetaryZones.Dtos;

public record GetAllMonetaryZonesRequest
{
    /// <summary>Optional page number (default = 1).</summary>
    public int? PageNumber { get; init; } = 1;

    /// <summary>Optional page size (default = 10).</summary>
    public int? PageSize { get; init; } = 10;

    /// <summary>Optional filter by code.</summary>
    public string? Code { get; init; }

    /// <summary>Optional filter by name.</summary>
    public string? Name { get; init; }

    /// <summary>Optional filter by description.</summary>
    public string? Description { get; init; }

    /// <summary>Optional filter by Status.</summary>
    public bool? IsEnabled { get; init; } = true;

}
