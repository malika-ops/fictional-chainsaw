using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.AgencyTiers.Dtos;
using wfc.referential.Application.Interfaces;

namespace wfc.referential.Application.AgencyTiers.Queries.GetAllAgencyTiers;

public class GetAllAgencyTiersQueryHandler : IQueryHandler<GetAllAgencyTiersQuery, PagedResult<AgencyTierResponse>>
{
    private readonly IAgencyTierRepository _repo;

    public GetAllAgencyTiersQueryHandler(IAgencyTierRepository repo) => _repo = repo;

    public async Task<PagedResult<AgencyTierResponse>> Handle(
        GetAllAgencyTiersQuery q, CancellationToken ct)
    {
        var list = await _repo.GetFilteredAgencyTiersAsync(q, ct);
        var total = await _repo.GetCountTotalAsync(q, ct);

        var dtos = list.Adapt<List<AgencyTierResponse>>();

        return new PagedResult<AgencyTierResponse>(dtos, total, q.PageNumber, q.PageSize);
    }
}
