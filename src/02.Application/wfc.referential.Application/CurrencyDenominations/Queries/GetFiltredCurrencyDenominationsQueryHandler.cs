using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.CurrencyDenominations.Dtos;
using wfc.referential.Application.CurrencyDenominations.Queries.GetFiltredCurrencies;
using wfc.referential.Application.Interfaces;

namespace wfc.referential.Application.CurrencyDenominations.Queries.GetFiltredCurrencyDenominations;

public class GetFiltredCurrencyDenominationsHandler
    : IQueryHandler<GetFiltredCurrencyDenominationsQuery, PagedResult<GetCurrencyDenominationsResponse>>
{
    private readonly ICurrencyRepository _repo;

    public GetFiltredCurrencyDenominationsHandler(ICurrencyRepository repo) => _repo = repo;

    public async Task<PagedResult<GetCurrencyDenominationsResponse>> Handle(
        GetFiltredCurrencyDenominationsQuery currencyQuery, CancellationToken ct)
    {
        var currencydenominations = await _repo.GetPagedByCriteriaAsync(currencyQuery, currencyQuery.PageNumber, currencyQuery.PageSize, ct);
        return new PagedResult<GetCurrencyDenominationsResponse>(currencydenominations.Items.Adapt<List<GetCurrencyDenominationsResponse>>(), currencydenominations.TotalCount, currencydenominations.PageNumber, currencydenominations.PageSize);
    }
}