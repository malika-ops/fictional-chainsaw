using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Caching.Interface;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.Corridors.Dtos;

namespace wfc.referential.Application.Corridors.Queries.GetFiltredCorridors;

public record GetFiltredCorridorsQuery : IQuery<PagedResult<GetCorridorResponse>>, ICacheableQuery
{
    public Guid? SourceCountryId { get; init; }
    public Guid? DestinationCountryId { get; init; }

    public Guid? SourceCityId { get; init; }
    public Guid? DestinationCityId { get; init; }

    public Guid? SourceBranchId { get; init; }
    public Guid? DestinationBranchId { get; init; }

    public bool? IsEnabled { get; init; }

    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;

    public string CacheKey =>
        $"Corridor_{SourceCountryId}_{DestinationCountryId}_" +
        $"{SourceCityId}_{DestinationCityId}_" +
        $"{SourceBranchId}_{DestinationBranchId}_" +
        $"{IsEnabled}_{PageNumber}_{PageSize}";

    public int CacheExpiration => 5;
}
