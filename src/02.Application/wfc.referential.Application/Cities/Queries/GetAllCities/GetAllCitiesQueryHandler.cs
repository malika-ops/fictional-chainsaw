using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Cities.Dtos;
using wfc.referential.Application.Interfaces;

namespace wfc.referential.Application.Cities.Queries.GetAllCities;

public class GetAllCitiesQueryHandler(ICityRepository _cityRepository, ICacheService _cacheService)
    : IQueryHandler<GetAllCitiesQuery, PagedResult<GetAllCitiesResponse>>
{

    public async Task<PagedResult<GetAllCitiesResponse>> Handle(GetAllCitiesQuery request, CancellationToken cancellationToken)
    {
        var cachedCities = await _cacheService.GetAsync<PagedResult<GetAllCitiesResponse>>(request.CacheKey, cancellationToken);
        if (cachedCities is not null)
        {
            return cachedCities;
        }

        var regions = await _cityRepository.GetCitiesByCriteriaAsync(request, cancellationToken);

        int totalCount = await _cityRepository.GetCountTotalAsync(request, cancellationToken);

        var regionsResponse = regions.Adapt<List<GetAllCitiesResponse>>();

        var result = new PagedResult<GetAllCitiesResponse>(regionsResponse, totalCount, request.PageNumber, request.PageSize);

        await _cacheService.SetAsync(request.CacheKey,
            result,
            TimeSpan.FromMinutes(request.CacheExpiration),
            cancellationToken);

        return result;
    }
}
