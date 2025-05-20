namespace wfc.referential.Application.Corridors.Dtos;

/// <summary>
/// Request DTO to fetch paginated corridors with optional filters.
/// </summary>
public record GetAllCorridorsRequest
{
    /// <summary>Filter by source country ID.</summary>
    /// <example>91c31591-b6a9-4cc0-9be6-79b2d3e5e211</example>
    public Guid? SourceCountryId { get; init; }

    /// <summary>Filter by destination country ID.</summary>
    /// <example>98d3b713-4097-4e68-9483-e4d6ab3b556f</example>
    public Guid? DestinationCountryId { get; init; }

    /// <summary>Filter by source city ID.</summary>
    public Guid? SourceCityId { get; init; }

    /// <summary>Filter by destination city ID.</summary>
    public Guid? DestinationCityId { get; init; }

    /// <summary>Filter by source agency ID.</summary>
    public Guid? SourceAgencyId { get; init; }

    /// <summary>Filter by destination agency ID.</summary>
    public Guid? DestinationAgencyId { get; init; }

    /// <summary>Filter by corridor status (enabled or disabled).</summary>
    public bool? IsEnabled { get; init; }

    /// <summary>Page number for pagination (default: 1).</summary>
    public int? PageNumber { get; init; } = 1;

    /// <summary>Page size for pagination (default: 10).</summary>
    public int? PageSize { get; init; } = 10;
}
