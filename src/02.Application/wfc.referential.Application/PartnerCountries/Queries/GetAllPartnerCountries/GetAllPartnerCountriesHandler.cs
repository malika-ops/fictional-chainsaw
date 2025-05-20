using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.PartnerCountries.Dtos;

namespace wfc.referential.Application.PartnerCountries.Queries.GetAllPartnerCountries;

public class GetAllPartnerCountriesHandler : IQueryHandler<GetAllPartnerCountriesQuery, PagedResult<PartnerCountryResponse>>
{
    private readonly IPartnerCountryRepository _repo;

    public GetAllPartnerCountriesHandler(IPartnerCountryRepository repo) => _repo = repo;

    public async Task<PagedResult<PartnerCountryResponse>> Handle(GetAllPartnerCountriesQuery q, CancellationToken ct)
    {
        var rows = await _repo.GetAllPaginatedAsyncFiltered(q, ct);
        var totalCount = await _repo.GetTotalCountAsync(q, ct);

        var dtos = rows.Adapt<List<PartnerCountryResponse>>();
        return new PagedResult<PartnerCountryResponse>(dtos, totalCount, q.PageNumber, q.PageSize);
    }
}