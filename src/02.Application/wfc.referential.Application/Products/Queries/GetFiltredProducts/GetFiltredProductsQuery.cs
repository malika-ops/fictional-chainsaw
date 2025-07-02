using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Caching.Interface;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.Products.Dtos;
using wfc.referential.Domain.ProductAggregate;

namespace wfc.referential.Application.Products.Queries.GetFiltredProducts;

public record GetFiltredProductsQuery : IQuery<PagedResult<GetProdcutsResponse>>, ICacheableQuery
{
    
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public string? Code { get; init; }
    public string? Name { get; init; }
    public bool? IsEnabled { get; init; }
    public string CacheKey => $"{nameof(Product)}_page{PageNumber}_size{PageSize}_code{Code}_name{Name}_status{IsEnabled}";
    public int CacheExpiration => 5;

}
