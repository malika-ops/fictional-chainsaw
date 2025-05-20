using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.Sectors.Dtos;

public record UpdateSectorRequest
{
    /// <summary>
    /// The ID of the Sector to update.
    /// </summary>
    /// <example>9d805d81-8g38-7d4e-1e0h-81849gg947f6</example>
    public Guid SectorId { get; init; }

    /// <summary>
    /// A unique code identifier for the Sector.
    /// </summary>
    /// <example>CASA-NORD</example>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// A human-readable name for the Sector.
    /// </summary>
    /// <example>Casablanca Nord</example>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The ID of the City this Sector belongs to.
    /// </summary>
    /// <example>7b583b69-6e16-5b2c-9c8f-69627ee725d4</example>
    public Guid CityId { get; init; }

    /// <summary>
    /// Whether the sector is enabled or disabled
    /// </summary>
    /// <example>true</example>
    public bool IsEnabled { get; init; } = true;
}