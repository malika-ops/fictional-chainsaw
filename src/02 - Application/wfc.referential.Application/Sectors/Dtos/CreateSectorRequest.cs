using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.Sectors.Dtos;

public record CreateSectorRequest
{
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
}