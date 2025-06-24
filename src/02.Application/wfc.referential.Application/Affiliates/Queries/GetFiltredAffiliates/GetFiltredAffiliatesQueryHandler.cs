using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.Affiliates.Dtos;

namespace wfc.referential.Application.Affiliates.Queries.GetFiltredAffiliates;

public class GetFiltredAffiliatesQueryHandler : IQueryHandler<GetFiltredAffiliatesQuery, PagedResult<GetAffiliatesResponse>>
{
    private readonly IAffiliateRepository _repo;

    public GetFiltredAffiliatesQueryHandler(IAffiliateRepository repo) => _repo = repo;

    public async Task<PagedResult<GetAffiliatesResponse>> Handle(
        GetFiltredAffiliatesQuery affiliateQuery, CancellationToken ct)
    {
        var affiliates = await _repo.GetPagedByCriteriaAsync(
            affiliateQuery,
            affiliateQuery.PageNumber,
            affiliateQuery.PageSize,
            ct);

        return new PagedResult<GetAffiliatesResponse>(
            affiliates.Items.Adapt<List<GetAffiliatesResponse>>(),
            affiliates.TotalCount,
            affiliates.PageNumber,
            affiliates.PageSize);
    }
}