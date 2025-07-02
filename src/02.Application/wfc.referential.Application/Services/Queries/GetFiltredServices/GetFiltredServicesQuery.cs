using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Caching.Interface;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.Services.Dtos;
using wfc.referential.Domain.ServiceAggregate;

namespace wfc.referential.Application.Services.Queries.GetFiltredServices;

public record GetFiltredServicesQuery : IQuery<PagedResult<GetServicesResponse>>, ICacheableQuery
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public string? Code { get; init; }
    public string? Name { get; init; }
    public bool? IsEnabled { get; init; }

    public string CacheKey => $"{nameof(Service)}_page{PageNumber}_size{PageSize}_code{Code}_name{Name}_status{IsEnabled}";
    public int CacheExpiration => 5;
}
