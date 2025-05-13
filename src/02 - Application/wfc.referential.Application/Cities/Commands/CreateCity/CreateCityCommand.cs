using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Caching.Interface;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.RegionAggregate;

namespace wfc.referential.Application.Cities.Commands.CreateCity;

public record CreateCityCommand : ICommand<Result<Guid>>, ICacheableQuery
{
    public CityId CityId { get; set; } = default!;
    public string CityCode { get; init; } = string.Empty;
    public string CityName { get; init; } = string.Empty;
    public string Abbreviation { get; init; } = string.Empty;
    public RegionId RegionId { get; init; } = default!;
    public string TimeZone { get; init; } = string.Empty;
    public string TaxZone { get; init; } = string.Empty;
    public bool IsEnabled { get; init; } = true;

    public string CacheKey => $"{nameof(City)}_{CityId}";
    public int CacheExpiration => 5;
}
