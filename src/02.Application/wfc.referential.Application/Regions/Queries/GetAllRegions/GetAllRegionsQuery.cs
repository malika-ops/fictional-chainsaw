using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Caching.Interface;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.RegionManagement.Dtos;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.RegionAggregate;

namespace wfc.referential.Application.RegionManagement.Queries.GetAllRegions;

public record GetAllRegionsQuery : IQuery<PagedResult<GetAllRegionsResponse>>, ICacheableQuery
{
    
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public string? Code { get; init; }
    public string? Name { get; init; }
    public CountryId? CountryId { get; init; }
    public bool? IsEnabled { get; init; }
    public string CacheKey => $"{nameof(Region)}_page{PageNumber}_size{PageSize}_code{Code}_name{Name}_status{IsEnabled}";
    public int CacheExpiration => 5;

}
