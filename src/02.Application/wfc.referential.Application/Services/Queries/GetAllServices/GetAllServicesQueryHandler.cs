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

        var services = await serviceRepository.GetPagedByCriteriaAsync(request, request.PageNumber,request.PageSize, cancellationToken);
        var result = new PagedResult<GetAllServicesResponse>(
            services.Items.Adapt<List<GetAllServicesResponse>>(),
            services.TotalCount, request.PageNumber, request.PageSize);

        return result;
    }
}