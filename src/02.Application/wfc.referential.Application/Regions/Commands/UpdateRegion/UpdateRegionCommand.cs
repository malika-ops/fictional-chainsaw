using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Caching.Interface;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.RegionAggregate;

namespace wfc.referential.Application.Regions.Commands.UpdateRegion;

public record UpdateRegionCommand : ICommand<Result<Guid>>, ICacheableQuery
{
    public Guid RegionId { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public bool IsEnabled { get; set; }
    public CountryId CountryId { get; set; }
    public string CacheKey => $"{nameof(Region)}_{RegionId}";
    public int CacheExpiration => 5;
}
