using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.PartnerCountries.Dtos;

namespace wfc.referential.Application.PartnerCountries.Queries.GetFiltredPartnerCountries;

public class GetFiltredPartnerCountriesHandler : IQueryHandler<GetFiltredPartnerCountriesQuery, PagedResult<PartnerCountryResponse>>
{
    private readonly IPartnerCountryRepository _repo;

    public GetFiltredPartnerCountriesHandler(IPartnerCountryRepository repo)
    {
        _repo = repo;
    }

    public async Task<PagedResult<PartnerCountryResponse>> Handle(GetFiltredPartnerCountriesQuery request, CancellationToken cancellationToken)
    {

        var partnerCountries = await _repo.GetPagedByCriteriaAsync(request,
           request.PageNumber,
           request.PageSize,
           cancellationToken);

        return new PagedResult<PartnerCountryResponse>(partnerCountries.Items.Adapt<List<PartnerCountryResponse>>(),
            partnerCountries.TotalCount,
            partnerCountries.PageNumber,
            partnerCountries.PageSize);
    }
}