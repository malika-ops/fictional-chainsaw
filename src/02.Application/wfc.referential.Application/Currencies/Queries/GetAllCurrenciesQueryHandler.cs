using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Currencies.Dtos;
using wfc.referential.Application.Interfaces;

namespace wfc.referential.Application.Currencies.Queries.GetAllCurrencies;

public class GetAllCurrenciesHandler
    : IQueryHandler<GetAllCurrenciesQuery, PagedResult<GetCurrenciesResponse>>
{
    private readonly ICurrencyRepository _repo;

    public GetAllCurrenciesHandler(ICurrencyRepository repo) => _repo = repo;

    public async Task<PagedResult<GetCurrenciesResponse>> Handle(
        GetAllCurrenciesQuery currencyQuery, CancellationToken ct)
    {
        var currencies = await _repo.GetPagedByCriteriaAsync(currencyQuery, currencyQuery.PageNumber, currencyQuery.PageSize, ct);
        return new PagedResult<GetCurrenciesResponse>(currencies.Items.Adapt<List<GetCurrenciesResponse>>(), currencies.TotalCount, currencies.PageNumber, currencies.PageSize);
    }
}