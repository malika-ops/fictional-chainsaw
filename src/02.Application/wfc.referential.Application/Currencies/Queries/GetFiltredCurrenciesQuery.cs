using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.Currencies.Dtos;

namespace wfc.referential.Application.Currencies.Queries.GetFiltredCurrencies;

public record GetFiltredCurrenciesQuery : IQuery<PagedResult<GetCurrenciesResponse>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;

    public string? Code { get; init; }
    public string? CodeAR { get; init; }
    public string? CodeEN { get; init; }
    public string? Name { get; init; }
    public int? CodeIso { get; init; }
    public bool? IsEnabled { get; init; } = true;
}