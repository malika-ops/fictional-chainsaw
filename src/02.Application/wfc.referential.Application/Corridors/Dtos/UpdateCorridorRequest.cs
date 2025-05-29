namespace wfc.referential.Application.Corridors.Dtos;

/// <summary>Full update of a corridor</summary>
public record UpdateCorridorRequest
{
    /// <summary>Corridor ID</summary>
    public Guid CorridorId { get; init; }

    /// <summary>Source country ID</summary>
    public Guid SourceCountryId { get; init; }

    /// <summary>Destination country ID</summary>
    public Guid DestinationCountryId { get; init; }

    /// <summary>Source city ID</summary>
    public Guid? SourceCityId { get; init; }

    /// <summary>Destination city ID</summary>
    public Guid? DestinationCityId { get; init; }

    /// <summary>Source agency ID</summary>
    public Guid? SourceBranchId { get; init; }

    /// <summary>Destination agency ID</summary>
    public Guid? DestinationBranchId { get; init; }

    /// <summary>Corridor enabled status</summary>
    public bool IsEnabled { get; init; }
}
