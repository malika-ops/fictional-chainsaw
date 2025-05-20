using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Agencies.Dtos;
using wfc.referential.Application.Interfaces;

namespace wfc.referential.Application.Agencies.Queries.GetAllAgencies;

public class GetAllAgenciesHandler
    : IQueryHandler<GetAllAgenciesQuery, PagedResult<GetAgenciesResponse>>
{
    private readonly IAgencyRepository _repo;

    public GetAllAgenciesHandler(IAgencyRepository repo) => _repo = repo;

    public async Task<PagedResult<GetAgenciesResponse>> Handle(
        GetAllAgenciesQuery q, CancellationToken ct)
    {
        var agencies = await _repo.GetAllAgenciesPaginatedAsyncFiltered(q, ct);
        var totalCount = await _repo.GetCountTotalAsync(q, ct);

        var dtos = agencies.Adapt<List<GetAgenciesResponse>>();
        return new PagedResult<GetAgenciesResponse>(dtos, totalCount, q.PageNumber, q.PageSize);
    }
}
