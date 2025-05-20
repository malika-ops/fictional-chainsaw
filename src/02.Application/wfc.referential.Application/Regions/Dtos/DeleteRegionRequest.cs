namespace wfc.referential.Application.Regions.Dtos;

public record DeleteRegionRequest
{
    /// <summary>
    /// The string representation of the Regions GUID.
    /// </summary>
    /// <example>6a472a58-5d05-4a1b-8b7f-58516dd614c3</example>
    public Guid RegionID { get; init; }
}
