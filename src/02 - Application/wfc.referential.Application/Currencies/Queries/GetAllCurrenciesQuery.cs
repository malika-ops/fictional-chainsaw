using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Caching.Interface;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.Currencies.Dtos;

namespace wfc.referential.Application.Currencies.Queries;

public record GetAllCurrenciesQuery : IQuery<PagedResult<CurrencyResponse>>, ICacheableQuery
{
    public int PageNumber { get; }
    public int PageSize { get; }
    public string? Code { get; init; }
    public string? CodeAR { get; init; }
    public string? CodeEN { get; init; }
    public string? Name { get; init; }
    public int? CodeIso { get; init; }
    public bool? IsEnabled { get; init; }
    public string CacheKey => $"currencies_page{PageNumber}_size{PageSize}_code{Code}_codeAR{CodeAR}_codeEN{CodeEN}_name{Name}_codeIso{CodeIso}_isEnabled{IsEnabled}";
    public int CacheExpiration => 10; // Cache for 10 minutes

    public GetAllCurrenciesQuery(
        int pageNumber,
        int pageSize,
        string? code = null,
        string? codeAR = null,
        string? codeEN = null,
        string? name = null,
        int? codeiso = null,
        bool? isEnabled = true)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        Code = code;
        CodeAR = codeAR;
        CodeEN = codeEN;
        Name = name;
        CodeIso = codeiso;
        IsEnabled = isEnabled;
    }
}