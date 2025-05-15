namespace wfc.referential.Application.Corridors.Dtos;

/// <summary>
/// Represents a single corridor in the result of a GetAllCorridors query.
/// </summary>
public record GetAllCorridorsResponse
{
    /// <summary>Corridor ID.</summary>
    public Guid? CorridorId { get; set; }

    /// <summary>Source country ID.</summary>
    public Guid? SourceCountryId { get; set; }

    /// <summary>Destination country ID.</summary>
    public Guid? DestinationCountryId { get; set; }

    /// <summary>Source city ID.</summary>
    public Guid? SourceCityId { get; set; }

    /// <summary>Destination city ID.</summary>
    public Guid? DestinationCityId { get; set; }

    /// <summary>Source agency ID.</summary>
    public Guid? SourceAgencyId { get; set; }

    /// <summary>Destination agency ID.</summary>
    public Guid? DestinationAgencyId { get; set; }

    /// <summary>Whether the corridor is enabled.</summary>
    public bool IsEnabled { get; set; }
}
