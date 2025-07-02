using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.Services.Dtos;

namespace wfc.referential.Application.Services.Queries.GetFiltredServices;

public class GetFiltredServicesQueryHandler(IServiceRepository serviceRepository, ICacheService cacheService)
    : IQueryHandler<GetFiltredServicesQuery, PagedResult<GetServicesResponse>>
{
    public async Task<PagedResult<GetServicesResponse>> Handle(GetFiltredServicesQuery request, CancellationToken cancellationToken)
    {
        var cached = await cacheService.GetAsync<PagedResult<GetServicesResponse>>(request.CacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        var services = await serviceRepository.GetPagedByCriteriaAsync(request, request.PageNumber,request.PageSize, cancellationToken);
        var result = new PagedResult<GetServicesResponse>(
            services.Items.Adapt<List<GetServicesResponse>>(),
            services.TotalCount, request.PageNumber, request.PageSize);

        return result;
    }
}