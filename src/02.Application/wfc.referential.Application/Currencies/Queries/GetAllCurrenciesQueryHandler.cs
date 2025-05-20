using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.Currencies.Dtos;
using wfc.referential.Application.Interfaces;

namespace wfc.referential.Application.Currencies.Queries;

public class GetAllCurrenciesQueryHandler(ICurrencyRepository currencyRepository, ICacheService cacheService)
    : IQueryHandler<GetAllCurrenciesQuery, PagedResult<CurrencyResponse>>
{
    public async Task<PagedResult<CurrencyResponse>> Handle(GetAllCurrenciesQuery request, CancellationToken cancellationToken)
    {
        // Check if the result is in cache
        //var cachedResult = await cacheService.GetAsync<PagedResult<CurrencyResponse>>(request.CacheKey, cancellationToken);
        //if (cachedResult != null)
        //{
        //    return cachedResult;
        //}

        // If not in cache, fetch from repository
        var currencies = await currencyRepository.GetCurrenciesByCriteriaAsync(request, cancellationToken);
        int totalCount = await currencyRepository.GetCountTotalAsync(request, cancellationToken);

        var currencyDtos = currencies.Select(c => new CurrencyResponse(
            c.Id.Value,
            c.Code,
            c.CodeAR,
            c.CodeEN,
            c.Name,
            c.CodeIso,
            c.IsEnabled,
            c.Countries?.Count ?? 0
        )).ToList();

        var result = new PagedResult<CurrencyResponse>(currencyDtos, totalCount, request.PageNumber, request.PageSize);

        //// Cache the result
        //await cacheService.SetAsync(
        //    request.CacheKey,
        //    result,
        //    TimeSpan.FromMinutes(request.CacheExpiration),
        //    cancellationToken);

        return result;
    }
}