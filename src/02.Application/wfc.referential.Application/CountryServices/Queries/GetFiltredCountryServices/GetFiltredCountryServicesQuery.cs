using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Caching.Interface;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.CountryServices.Dtos;

namespace wfc.referential.Application.CountryServices.Queries.GetFiltredCountryServices;

public record GetFiltredCountryServicesQuery : IQuery<PagedResult<GetCountryServicesResponse>>, ICacheableQuery
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public Guid? CountryId { get; init; }
    public Guid? ServiceId { get; init; }
    public bool? IsEnabled { get; init; } = true;
    public string CacheKey =>
        $"CountryService_{CountryId}_{ServiceId}_" +
        $"{IsEnabled}_{PageNumber}_{PageSize}";
    public int CacheExpiration => 5;
}