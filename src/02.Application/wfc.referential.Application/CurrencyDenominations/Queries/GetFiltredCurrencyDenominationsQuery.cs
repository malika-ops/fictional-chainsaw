using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.CurrencyDenominations.Dtos;
using wfc.referential.Domain.CurrencyDenominationAggregate;

namespace wfc.referential.Application.CurrencyDenominations.Queries.GetFiltredCurrencies;

public record GetFiltredCurrencyDenominationsQuery : IQuery<PagedResult<GetCurrencyDenominationsResponse>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;

    public Guid? CurrencyId { get; init; }
    public CurrencyDenominationType? Type { get; init; }
    public decimal? Value { get; init; }
    public bool? IsEnabled { get; init; } = true;
}