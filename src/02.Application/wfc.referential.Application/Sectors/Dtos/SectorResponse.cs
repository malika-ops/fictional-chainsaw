namespace wfc.referential.Application.Sectors.Dtos;

public record SectorResponse
{
    /// <summary>
    /// Unique identifier of the sector.
    /// </summary>
    /// <example>e3b0c442-98fc-1c14-9afb-4c1a1e1f2a3b</example>
    public Guid SectorId { get; init; }

    /// <summary>
    /// Unique code of the sector.
    /// </summary>
    /// <example>SEC001</example>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// Name of the sector.
    /// </summary>
    /// <example>Retail</example>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Unique identifier of the city associated with the sector.
    /// </summary>
    /// <example>f1e2d3c4-b5a6-7890-1234-56789abcdef0</example>
    public Guid CityId { get; init; }

    /// <summary>
    /// Name of the city associated with the sector.
    /// </summary>
    /// <example>Casablanca</example>
    public string? CityName { get; init; } = string.Empty;

    /// <summary>
    /// Indicates whether the sector is enabled.
    /// </summary>
    /// <example>true</example>
    public bool IsEnabled { get; init; }
}