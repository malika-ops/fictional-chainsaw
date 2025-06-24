using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Caching.Interface;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.Cities.Dtos;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.RegionAggregate;

namespace wfc.referential.Application.Cities.Queries.GetFiltredCities;
public record GetFiltredCitiesQuery : IQuery<PagedResult<GetCitiyResponse>>, ICacheableQuery
{   
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Abbreviation { get; set; }
    public string? TaxZone { get; set; }
    public string? TimeZone { get; set; }
    public RegionId? RegionId { get; set; }
    public bool? IsEnabled { get; set; }
    public string CacheKey => $"{nameof(City)}_page{PageNumber}_size{PageSize}_code{Code}_name{Name}_status{IsEnabled}";
    public int CacheExpiration => 5;
}
