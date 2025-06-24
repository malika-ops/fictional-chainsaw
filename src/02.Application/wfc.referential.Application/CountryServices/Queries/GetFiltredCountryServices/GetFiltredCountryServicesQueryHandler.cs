using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.CountryServices.Dtos;
using wfc.referential.Application.Interfaces;

namespace wfc.referential.Application.CountryServices.Queries.GetFiltredCountryServices;

public class GetFiltredCountryServicesQueryHandler(ICountryServiceRepository _countryServiceRepository) 
    : IQueryHandler<GetFiltredCountryServicesQuery, PagedResult<GetCountryServicesResponse>>
{

    public async Task<PagedResult<GetCountryServicesResponse>> Handle(GetFiltredCountryServicesQuery query, CancellationToken ct)
    {
        var countryServices = await _countryServiceRepository.GetPagedByCriteriaAsync(
            query,
            query.PageNumber,
            query.PageSize,
            ct);

        return new PagedResult<GetCountryServicesResponse>(
            countryServices.Items.Adapt<List<GetCountryServicesResponse>>(),
            countryServices.TotalCount,
            countryServices.PageNumber,
            countryServices.PageSize);
    }
}