namespace wfc.referential.Application.Sectors.Dtos;

public record GetAllSectorsRequest
{
    /// <summary>Page number (default = 1).</summary>
    public int? PageNumber { get; init; } = 1;

    /// <summary>Page size (default = 10).</summary>
    public int? PageSize { get; init; } = 10;

    /// <summary>Filter by sector code.</summary>
    public string? Code { get; init; }

    /// <summary>Filter by sector name.</summary>
    public string? Name { get; init; }

    /// <summary>Filter by city ID.</summary>
    public Guid? CityId { get; init; }

    /// <summary>Status filter (Enabled/Disabled).</summary>
    public bool? IsEnabled { get; init; } = true;
}