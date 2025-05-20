using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.Sectors.Dtos;

public record DeleteSectorRequest
{
    /// <summary>
    /// The ID of the Sector to delete.
    /// </summary>
    /// <example>9d805d81-8g38-7d4e-1e0h-81849gg947f6</example>
    public Guid SectorId { get; init; }
}