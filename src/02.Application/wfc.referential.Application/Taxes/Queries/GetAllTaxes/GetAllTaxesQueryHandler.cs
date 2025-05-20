using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.Taxes.Dtos;

namespace wfc.referential.Application.Taxes.Queries.GetAllTaxes;

public class GetAllTaxesQueryHandler(
    ITaxRepository _taxRepository,
    ICacheService _cacheService
) : IQueryHandler<GetAllTaxesQuery, PagedResult<GetAllTaxesResponse>>
{
    public async Task<PagedResult<GetAllTaxesResponse>> Handle(GetAllTaxesQuery request, CancellationToken cancellationToken)
    {
        var cachedTaxes = await _cacheService.GetAsync<PagedResult<GetAllTaxesResponse>>(request.CacheKey, cancellationToken);
        if (cachedTaxes is not null)
        {
            return cachedTaxes;
        }

        var taxes = await _taxRepository
            .GetTaxesByCriteriaAsync(request, cancellationToken);

        int totalCount = await _taxRepository
            .GetCountTotalAsync(request, cancellationToken);

        var taxesResponse = taxes.Adapt<List<GetAllTaxesResponse>>();

        var result = new PagedResult<GetAllTaxesResponse>(
            taxesResponse,
            totalCount,
            request.PageNumber,
            request.PageSize
        );

        await _cacheService.SetAsync(
            request.CacheKey,
            result,
            TimeSpan.FromMinutes(request.CacheExpiration),
            cancellationToken
        );

        return result;
    }
}
