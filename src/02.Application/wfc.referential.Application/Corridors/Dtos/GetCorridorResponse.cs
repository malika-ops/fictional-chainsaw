namespace wfc.referential.Application.Corridors.Dtos;

/// <summary>
/// Represents a single corridor in the result of a GetFiltredCorridors query.
/// </summary>
public record GetCorridorResponse
{
    /// <summary>Corridor ID.</summary>
    /// <example>e2a1c3b4-5d6f-4a7b-8c9d-0e1f2a3b4c5d</example>
    public Guid? CorridorId { get; set; }

    /// <summary>Source country ID.</summary>
    /// <example>f1e2d3c4-b5a6-7890-1234-56789abcdef0</example>
    public Guid? SourceCountryId { get; set; }

    /// <summary>Destination country ID.</summary>
    /// <example>0a1b2c3d-4e5f-6789-abcd-ef0123456789</example>
    public Guid? DestinationCountryId { get; set; }

    /// <summary>Source city ID.</summary>
    /// <example>c1a2b3d4-e5f6-7890-abcd-1234567890ef</example>
    public Guid? SourceCityId { get; set; }

    /// <summary>Destination city ID.</summary>
    /// <example>f7e6d5c4-b3a2-1098-7654-3210fedcba98</example>
    public Guid? DestinationCityId { get; set; }

    /// <summary>Source agency ID.</summary>
    /// <example>e3b0c442-98fc-1c14-9afb-4c1a1e1f2a3b</example>
    public Guid? SourceBranchId { get; set; }

    /// <summary>Destination agency ID.</summary>
    /// <example>e2a1c3b4-5d6f-4a7b-8c9d-0e1f2a3b4c5d</example>
    public Guid? DestinationBranchId { get; set; }

    /// <summary>Whether the corridor is enabled.</summary>
    /// <example>true</example>
    public bool IsEnabled { get; set; }
}
