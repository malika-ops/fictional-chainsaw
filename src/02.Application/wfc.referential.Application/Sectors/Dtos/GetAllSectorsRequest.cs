namespace wfc.referential.Application.Sectors.Dtos;

public record GetAllSectorsRequest
{
    /// <summary>Optional page number (default = 1).</summary>
    public int? PageNumber { get; init; } = 1;

    /// <summary>Optional page size (default = 10).</summary>
    public int? PageSize { get; init; } = 10;

    /// <summary>Optional filter by code.</summary>
    public string? Code { get; init; }

    /// <summary>Optional filter by name.</summary>
    public string? Name { get; init; }

    /// <summary>Optional filter by City ID.</summary>
    public Guid? CityId { get; init; }

    /// <summary>Optional filter by enabled status.</summary>
    public bool? IsEnabled { get; init; } = true;
}