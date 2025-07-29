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
    private readonly ICurrencyDenominationRepository _repo;

    public GetFiltredCurrencyDenominationsHandler(ICurrencyDenominationRepository repo) => _repo = repo;

    public async Task<PagedResult<GetCurrencyDenominationsResponse>> Handle(
        GetFiltredCurrencyDenominationsQuery currencyDenominationQuery, CancellationToken ct)
    {
        var currencydenominations = await _repo.GetPagedByCriteriaAsync(currencyDenominationQuery, currencyDenominationQuery.PageNumber, currencyDenominationQuery.PageSize, ct);
        return new PagedResult<GetCurrencyDenominationsResponse>(currencydenominations.Items.Adapt<List<GetCurrencyDenominationsResponse>>(), currencydenominations.TotalCount, currencydenominations.PageNumber, currencydenominations.PageSize);
    }
}