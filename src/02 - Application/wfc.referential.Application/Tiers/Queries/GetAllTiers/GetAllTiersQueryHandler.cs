using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.Tiers.Dtos;

namespace wfc.referential.Application.Tiers.Queries.GetAllTiers;

public class GetAllTiersQueryHandler : IQueryHandler<GetAllTiersQuery, PagedResult<TierResponse>>
{
    private readonly ITierRepository _repo;
    public GetAllTiersQueryHandler(ITierRepository repo) => _repo = repo;

    public async Task<PagedResult<TierResponse>> Handle(GetAllTiersQuery q, CancellationToken ct)
    {
        var tiers = await _repo.GetFilteredTiersAsync(q, ct);
        var total = await _repo.GetCountTotalAsync(q, ct);

        var dto = tiers.Adapt<List<TierResponse>>();
        return new PagedResult<TierResponse>(dto, total, q.PageNumber, q.PageSize);
    }
}