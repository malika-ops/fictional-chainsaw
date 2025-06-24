using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.Services.Dtos;

namespace wfc.referential.Application.Services.Queries.GetFiltredServices;

public class GetFiltredServicesQueryHandler(IServiceRepository serviceRepository, ICacheService cacheService)
    : IQueryHandler<GetFiltredServicesQuery, PagedResult<GetFiltredServicesResponse>>
{
    public async Task<PagedResult<GetFiltredServicesResponse>> Handle(GetFiltredServicesQuery request, CancellationToken cancellationToken)
    {
        var cached = await cacheService.GetAsync<PagedResult<GetFiltredServicesResponse>>(request.CacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        var services = await serviceRepository.GetPagedByCriteriaAsync(request, request.PageNumber,request.PageSize, cancellationToken);
        var result = new PagedResult<GetFiltredServicesResponse>(
            services.Items.Adapt<List<GetFiltredServicesResponse>>(),
            services.TotalCount, request.PageNumber, request.PageSize);

        return result;
    }
}