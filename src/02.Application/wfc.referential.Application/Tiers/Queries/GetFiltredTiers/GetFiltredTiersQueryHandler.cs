using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.Tiers.Dtos;

namespace wfc.referential.Application.Tiers.Queries.GetFiltredTiers;

public class GetFiltredTiersQueryHandler : IQueryHandler<GetFiltredTiersQuery, PagedResult<TierResponse>>
{
    private readonly ITierRepository _tierRepo;
    public GetFiltredTiersQueryHandler(ITierRepository tierRepo)
    {
        _tierRepo = tierRepo;
    }

    public async Task<PagedResult<TierResponse>> Handle(GetFiltredTiersQuery tierQuery, CancellationToken ct)
    {
        var tiers = await _tierRepo.GetPagedByCriteriaAsync(tierQuery, 
            tierQuery.PageNumber, 
            tierQuery.PageSize, 
            ct);

        return new PagedResult<TierResponse>(tiers.Items.Adapt<List<TierResponse>>(), 
            tiers.TotalCount, 
            tiers.PageNumber, 
            tiers.PageSize);
    }
}