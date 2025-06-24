using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.AgencyTiers.Dtos;

namespace wfc.referential.Application.AgencyTiers.Queries.GetFiltredAgencyTiers;

public class GetFiltredAgencyTiersQuery : IQuery<PagedResult<AgencyTierResponse>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;

    public Guid? AgencyId { get; init; }
    public Guid? TierId { get; init; }
    public string? Code { get; init; }
    public bool? IsEnabled { get; init; } = true;
}