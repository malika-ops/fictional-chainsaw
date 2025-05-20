using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Caching.Interface;
using wfc.referential.Domain.RegionAggregate;

namespace wfc.referential.Application.Regions.Commands.DeleteRegion;

public record DeleteRegionCommand : ICommand<Result<bool>>, ICacheableQuery
{
    public Guid RegionId { get; init; }
    public string CacheKey => $"{nameof(Region)}_{RegionId}";
    public int CacheExpiration => 5;
}