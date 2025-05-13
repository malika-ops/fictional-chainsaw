using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.Sectors.Dtos;

public record PatchSectorRequest
{
    /// <summary>
    /// The ID of the Sector to patch.
    /// </summary>
    /// <example>9d805d81-8g38-7d4e-1e0h-81849gg947f6</example>
    public Guid SectorId { get; init; }

    /// <summary>
    /// If provided, updates the code. If omitted, code remains unchanged.
    /// </summary>
    /// <example>CASA-NORD-UPDATED</example>
    public string? Code { get; init; }

    /// <summary>
    /// If provided, updates the name. If omitted, name remains unchanged.
    /// </summary>
    /// <example>Casablanca Nord Updated</example>
    public string? Name { get; init; }

    /// <summary>
    /// If provided, updates the City. If omitted, City remains unchanged.
    /// </summary>
    /// <example>7b583b69-6e16-5b2c-9c8f-69627ee725d4</example>
    public Guid? CityId { get; init; }

    /// <summary> 
    /// If provided, updates the enabled status. If omitted, status remains unchanged. 
    /// </summary>
    public bool? IsEnabled { get; init; }
}