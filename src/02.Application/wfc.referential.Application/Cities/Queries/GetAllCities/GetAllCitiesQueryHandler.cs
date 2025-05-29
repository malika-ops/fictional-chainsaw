using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Cities.Dtos;
using wfc.referential.Application.Interfaces;

namespace wfc.referential.Application.Cities.Queries.GetAllCities;

public class GetAllCitiesQueryHandler(ICityRepository _cityRepository)
    : IQueryHandler<GetAllCitiesQuery, PagedResult<GetAllCitiesResponse>>
{

    public async Task<PagedResult<GetAllCitiesResponse>> Handle(GetAllCitiesQuery request, CancellationToken cancellationToken)
    {
        var cities = await _cityRepository.GetPagedByCriteriaAsync(request, request.PageNumber, request.PageSize, cancellationToken);
        var result = new PagedResult<GetAllCitiesResponse>(
            cities.Items.Adapt<List<GetAllCitiesResponse>>(),
            cities.TotalCount, request.PageNumber, request.PageSize);
        return result;
    }
}
