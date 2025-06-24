using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Agencies.Dtos;
using wfc.referential.Application.Interfaces;

namespace wfc.referential.Application.Agencies.Queries.GetFiltredAgencies;

public class GetFiltredAgenciesHandler
    : IQueryHandler<GetFiltredAgenciesQuery, PagedResult<GetAgenciesResponse>>
{
    private readonly IAgencyRepository _repo;

    public GetFiltredAgenciesHandler(IAgencyRepository repo) => _repo = repo;

    public async Task<PagedResult<GetAgenciesResponse>> Handle(
        GetFiltredAgenciesQuery agencyQuery, CancellationToken ct)
    {

        var page = await _repo.GetPagedByCriteriaAsync(agencyQuery, agencyQuery.PageNumber, agencyQuery.PageSize, ct);

        return new PagedResult<GetAgenciesResponse>(
            page.Items.Adapt<List<GetAgenciesResponse>>(),
            page.TotalCount,
            page.PageNumber,
            page.PageSize);

    }
}
