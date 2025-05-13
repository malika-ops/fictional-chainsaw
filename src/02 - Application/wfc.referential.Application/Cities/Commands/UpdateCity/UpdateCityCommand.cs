using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Caching.Interface;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.RegionAggregate;

namespace wfc.referential.Application.Cities.Commands.UpdateCity;

public record UpdateCityCommand : ICommand<Result<Guid>>, ICacheableQuery
{
    public Guid CityId { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Abbreviation { get; set; } = default!;
    public string TaxZone { get; set; } = default!;
    public string TimeZone { get;  set; } = default!;
    public bool IsEnabled { get; set; }
    public RegionId? RegionId { get; set; } = default!;
    public string CacheKey => $"{nameof(City)}_{CityId}";
    public int CacheExpiration => 5;
}
