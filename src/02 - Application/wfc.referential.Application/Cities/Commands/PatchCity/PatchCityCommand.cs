using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Caching.Interface;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.RegionAggregate;

namespace wfc.referential.Application.Cities.Commands.PatchCity;

public record PatchCityCommand : ICommand<Result<Guid>>, ICacheableQuery
{
    // The ID from the route
    public Guid CityId { get; init; }
    public string? Code { get; init; }
    public string? Name { get; init; }
    public string? Abbreviation { get; init; }
    public string? TaxZone { get; init; }
    public string? TimeZone { get; init; }
    public bool? IsEnabled { get; init; }
    public RegionId? RegionId { get; init; }
    public string CacheKey => $"{nameof(City)}_{CityId}";
    public int CacheExpiration => 5;
}
