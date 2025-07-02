using wfc.referential.Application.Cities.Dtos;

namespace wfc.referential.Application.RegionManagement.Dtos;

public record GetRegionsResponse
{
    /// <summary>
    /// Unique identifier of the region.
    /// </summary>
    /// <example>e2a1c3b4-5d6f-4a7b-8c9d-0e1f2a3b4c5d</example>
    public Guid RegionId { get; init; }

    /// <summary>
    /// Unique code of the region.
    /// </summary>
    /// <example>REG001</example>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// Name of the region.
    /// </summary>
    /// <example>Greater Casablanca</example>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Indicates whether the region is enabled.
    /// </summary>
    /// <example>true</example>
    public bool IsEnabled { get; init; }

    /// <summary>
    /// Unique identifier of the country associated with the region.
    /// </summary>
    /// <example>f1e2d3c4-b5a6-7890-1234-56789abcdef0</example>
    public Guid CountryId { get; init; }

    /// <summary>
    /// List of cities associated with the region.
    /// </summary>
    public List<GetCitiyResponse>? Cities { get; init; }
}
