namespace wfc.referential.Application.Corridors.Dtos;

/// <summary>
/// Represents a single corridor in the result of a GetFiltredCorridors query.
/// </summary>
public record GetCorridorResponse
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
    public Guid? SourceBranchId { get; set; }

    /// <summary>Destination agency ID.</summary>
    public Guid? DestinationBranchId { get; set; }

    /// <summary>Whether the corridor is enabled.</summary>
    public bool IsEnabled { get; set; }
}
