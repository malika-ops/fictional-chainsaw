using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Cities.Dtos;
using wfc.referential.Application.Interfaces;

namespace wfc.referential.Application.Cities.Queries.GetFiltredCities;

public class GetFiltredCitiesQueryHandler(ICityRepository _cityRepository)
    : IQueryHandler<GetFiltredCitiesQuery, PagedResult<GetCitiyResponse>>
{

    public async Task<PagedResult<GetCitiyResponse>> Handle(GetFiltredCitiesQuery request, CancellationToken cancellationToken)
    {
        var cities = await _cityRepository.GetPagedByCriteriaAsync(request, request.PageNumber, request.PageSize, cancellationToken);
        var result = new PagedResult<GetCitiyResponse>(
            cities.Items.Adapt<List<GetCitiyResponse>>(),
            cities.TotalCount, request.PageNumber, request.PageSize);
        return result;
    }
}
