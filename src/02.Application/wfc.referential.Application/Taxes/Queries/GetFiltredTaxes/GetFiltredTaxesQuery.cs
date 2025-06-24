using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Caching.Interface;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.Taxes.Dtos;
using wfc.referential.Domain.TaxAggregate;

namespace wfc.referential.Application.Taxes.Queries.GetFiltredTaxes;

/// <summary>
/// Query to retrieve a paginated and filterable list of taxes.
/// </summary>
public record GetFiltredTaxesQuery : IQuery<PagedResult<GetTaxesResponse>>, ICacheableQuery
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }

    public string? Code { get; init; }
    public string? CodeEn { get; init; }
    public string? CodeAr { get; init; }
    public string? Description { get; init; }
    public double? FixedAmount { get; init; }
    public double? Value { get; init; }
    public bool IsEnabled { get; init; }

    public string CacheKey =>
        $"{nameof(Tax)}_page{PageNumber}_size{PageSize}_code{Code}_codeEn{CodeEn}_codeAr{CodeAr}_desc{Description}_amount{FixedAmount}_value{Value}_isEnabled{IsEnabled}";

    public int CacheExpiration => 5;
}
