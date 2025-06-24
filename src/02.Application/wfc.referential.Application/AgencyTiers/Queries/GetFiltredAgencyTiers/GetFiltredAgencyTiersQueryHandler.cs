using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.AgencyTiers.Dtos;
using wfc.referential.Application.Interfaces;

namespace wfc.referential.Application.AgencyTiers.Queries.GetFiltredAgencyTiers;

public class GetFiltredAgencyTiersQueryHandler : IQueryHandler<GetFiltredAgencyTiersQuery, PagedResult<AgencyTierResponse>>
{
    private readonly IAgencyTierRepository _agencyTierRepo;

    public GetFiltredAgencyTiersQueryHandler(IAgencyTierRepository agencyTierRepo)
    {
        _agencyTierRepo = agencyTierRepo;
    }

    public async Task<PagedResult<AgencyTierResponse>> Handle(GetFiltredAgencyTiersQuery agencyTierQuery, CancellationToken ct)
    {
        var agencyTiers = await _agencyTierRepo.GetPagedByCriteriaAsync(agencyTierQuery,
                agencyTierQuery.PageNumber,
                agencyTierQuery.PageSize,
                ct);

        return new PagedResult<AgencyTierResponse>(agencyTiers.Items.Adapt<List<AgencyTierResponse>>(),
            agencyTiers.TotalCount,
            agencyTiers.PageNumber,
            agencyTiers.PageSize);
    }
}
