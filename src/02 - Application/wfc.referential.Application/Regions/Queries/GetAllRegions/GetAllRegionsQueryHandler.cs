using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.RegionManagement.Dtos;

namespace wfc.referential.Application.RegionManagement.Queries.GetAllRegions;

public class GetAllRegionsQueryHandler(IRegionRepository regionRepository, ICacheService cacheService)
    : IQueryHandler<GetAllRegionsQuery, PagedResult<GetAllRegionsResponse>>
{
    private readonly IRegionRepository _regionRepository = regionRepository;
    private readonly ICacheService _cacheService = cacheService;

    public async Task<PagedResult<GetAllRegionsResponse>> Handle(GetAllRegionsQuery request, CancellationToken cancellationToken)
    {
        var cachedRegions = await _cacheService.GetAsync<PagedResult<GetAllRegionsResponse>>(request.CacheKey, cancellationToken);
        if (cachedRegions is not null)
        {
            return cachedRegions;
        }

        var regions = await _regionRepository
        .GetRegionsByCriteriaAsync(request, cancellationToken);

        int totalCount = await _regionRepository
            .GetCountTotalAsync(request, cancellationToken);

        var regionsResponse = regions.Adapt<List<GetAllRegionsResponse>>();

        var result = new PagedResult<GetAllRegionsResponse>(regionsResponse, totalCount, request.PageNumber, request.PageSize);

        await _cacheService.SetAsync(request.CacheKey, 
            result, 
            TimeSpan.FromMinutes(request.CacheExpiration), 
            cancellationToken);

        return result;
    }
}
