using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.Services.Dtos;

namespace wfc.referential.Application.Services.Queries.GetAllServices;

public class GetAllServicesQueryHandler(IServiceRepository serviceRepository, ICacheService cacheService)
    : IQueryHandler<GetAllServicesQuery, PagedResult<GetAllServicesResponse>>
{
    public async Task<PagedResult<GetAllServicesResponse>> Handle(GetAllServicesQuery request, CancellationToken cancellationToken)
    {
        var cached = await cacheService.GetAsync<PagedResult<GetAllServicesResponse>>(request.CacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        var entities = await serviceRepository.GetServicesByCriteriaAsync(request, cancellationToken);
        int totalCount = await serviceRepository.GetCountTotalAsync(request, cancellationToken);

        var mapped = entities.Adapt<List<GetAllServicesResponse>>();
        var result = new PagedResult<GetAllServicesResponse>(mapped, totalCount, request.PageNumber, request.PageSize);

        await cacheService.SetAsync(request.CacheKey, result, TimeSpan.FromMinutes(request.CacheExpiration), cancellationToken);

        return result;
    }
}