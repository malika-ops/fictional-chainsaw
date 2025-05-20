namespace wfc.referential.Application.AgencyTiers.Dtos;

public record GetAllAgencyTiersRequest
{
    /// <summary>Page number (default = 1).</summary>
    public int? PageNumber { get; init; } = 1;

    /// <summary>Page size  (default = 10).</summary>
    public int? PageSize { get; init; } = 10;

    /// <summary>Filter by AgencyId.</summary>
    public Guid? AgencyId { get; init; }

    /// <summary>Filter by TierId.</summary>
    public Guid? TierId { get; init; }

    /// <summary>Filter by mapping Code.</summary>
    public string? Code { get; init; }

    /// <summary>Optional status filter.</summary>
    public bool? IsEnabled { get; init; } = true;
}
