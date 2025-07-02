using wfc.referential.Application.Sectors.Dtos;

namespace wfc.referential.Application.Cities.Dtos;

public record GetCitiyResponse
{
    /// <summary>
    /// Unique identifier of the city.
    /// </summary>
    /// <example>e2a1c3b4-5d6f-4a7b-8c9d-0e1f2a3b4c5d</example>
    public Guid CityId { get; init; }

    /// <summary>
    /// Unique code of the city.
    /// </summary>
    /// <example>CITY001</example>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// Name of the city.
    /// </summary>
    /// <example>Casablanca</example>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Abbreviation of the city name.
    /// </summary>
    /// <example>CSB</example>
    public string Abbreviation { get; init; } = string.Empty;

    /// <summary>
    /// Time zone of the city.
    /// </summary>
    /// <example>Africa/Casablanca</example>
    public string TimeZone { get; init; } = string.Empty;

    /// <summary>
    /// Unique identifier of the region associated with the city.
    /// </summary>
    /// <example>f1e2d3c4-b5a6-7890-1234-56789abcdef0</example>
    public Guid RegionId { get; init; }

    /// <summary>
    /// Indicates whether the city is enabled.
    /// </summary>
    /// <example>true</example>
    public bool IsEnabled { get; init; }

    /// <summary>
    /// List of sectors associated with the city.
    /// </summary>
    public List<SectorResponse> Sectors { get; init; } = new();
}
