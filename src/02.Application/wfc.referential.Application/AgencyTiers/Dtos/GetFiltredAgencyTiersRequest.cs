namespace wfc.referential.Application.AgencyTiers.Dtos;

public record GetFiltredAgencyTiersRequest : FilterRequest
{
    /// <summary>Filter by AgencyId.</summary>
    public Guid? AgencyId { get; init; }

    /// <summary>Filter by TierId.</summary>
    public Guid? TierId { get; init; }

    /// <summary>Filter by mapping Code.</summary>
    public string? Code { get; init; }
}
