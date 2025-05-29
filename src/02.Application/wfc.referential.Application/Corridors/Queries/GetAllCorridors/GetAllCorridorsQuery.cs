using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Caching.Interface;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.Corridors.Dtos;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.CityAggregate;

namespace wfc.referential.Application.Corridors.Queries.GetAllCorridors;

public record GetAllCorridorsQuery : IQuery<PagedResult<GetAllCorridorsResponse>>, ICacheableQuery
{
    public CountryId? SourceCountryId { get; init; }
    public CountryId? DestinationCountryId { get; init; }

    public CityId? SourceCityId { get; init; }
    public CityId? DestinationCityId { get; init; }

    public AgencyId? SourceBranchId { get; init; }
    public AgencyId? DestinationBranchId { get; init; }

    public bool? IsEnabled { get; init; }

    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;

    public string CacheKey =>
        $"Corridors_{SourceCountryId?.Value}_{DestinationCountryId?.Value}_" +
        $"{SourceCityId?.Value}_{DestinationCityId?.Value}_" +
        $"{SourceBranchId?.Value}_{DestinationBranchId?.Value}_" +
        $"{IsEnabled}_{PageNumber}_{PageSize}";

    public int CacheExpiration => 5;
}
