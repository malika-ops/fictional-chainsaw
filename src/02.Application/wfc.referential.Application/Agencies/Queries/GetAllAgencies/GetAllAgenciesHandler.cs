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
        GetAllAgenciesQuery agencyQuery, CancellationToken ct)
    {
        var agencies = await _repo.GetPagedByCriteriaAsync(agencyQuery, agencyQuery.PageNumber, agencyQuery.PageSize, ct);
        return new PagedResult<GetAgenciesResponse>(agencies.Items.Adapt< List<GetAgenciesResponse>>(), agencies.TotalCount, agencies.PageNumber, agencies.PageSize);
    }
}
