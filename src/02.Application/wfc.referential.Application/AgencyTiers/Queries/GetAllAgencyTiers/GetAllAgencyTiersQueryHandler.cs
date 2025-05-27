using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.AgencyTiers.Dtos;
using wfc.referential.Application.Interfaces;

namespace wfc.referential.Application.AgencyTiers.Queries.GetAllAgencyTiers;

public class GetAllAgencyTiersQueryHandler : IQueryHandler<GetAllAgencyTiersQuery, PagedResult<AgencyTierResponse>>
{
    private readonly IAgencyTierRepository _agencyTierRepo;

    public GetAllAgencyTiersQueryHandler(IAgencyTierRepository agencyTierRepo)
    {
        _agencyTierRepo = agencyTierRepo;
    }

    public async Task<PagedResult<AgencyTierResponse>> Handle(GetAllAgencyTiersQuery agencyTierQuery, CancellationToken ct)
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
