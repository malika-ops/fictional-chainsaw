using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Corridors.Dtos;
using wfc.referential.Application.Interfaces;

namespace wfc.referential.Application.Corridors.Queries.GetAllCorridors;

public class GetAllCorridorsQueryHandler(ICorridorRepository _corridorRepository, ICacheService _cacheService)
    : IQueryHandler<GetAllCorridorsQuery, PagedResult<GetAllCorridorsResponse>>
{
    public async Task<PagedResult<GetAllCorridorsResponse>> Handle(GetAllCorridorsQuery request, CancellationToken cancellationToken)
    {
        var cachedCorridors = await _cacheService.GetAsync<PagedResult<GetAllCorridorsResponse>>(request.CacheKey, cancellationToken);
        if (cachedCorridors is not null)
        {
            return cachedCorridors;
        }

        var corridors = await _corridorRepository
        .GetCorridorsByCriteriaAsync(request, cancellationToken);

        int totalCount = await _corridorRepository
            .GetCountTotalAsync(request, cancellationToken);

        var corridorsResponse = corridors.Adapt<List<GetAllCorridorsResponse>>();

        var result = new PagedResult<GetAllCorridorsResponse>(corridorsResponse, totalCount, request.PageNumber, request.PageSize);

        await _cacheService.SetAsync(request.CacheKey, 
            result, 
            TimeSpan.FromMinutes(request.CacheExpiration), 
            cancellationToken);

        return result;
    }
}
