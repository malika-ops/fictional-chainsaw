namespace wfc.referential.Application.Sectors.Dtos;

public record DeleteSectorRequest
{
    /// <summary>GUID of the sector to delete.</summary>
    public Guid SectorId { get; init; }
}