using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Caching.Interface;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.RegionAggregate;

namespace wfc.referential.Application.Regions.Commands.PatchRegion;

public record PatchRegionCommand : ICommand<Result<Guid>>, ICacheableQuery
{
    // The ID from the route
    public Guid RegionId { get; init; }

    // The optional fields to update
    public string? Code { get; init; }
    public string? Name { get; init; }
    public bool? IsEnabled { get; init; }
    public CountryId? CountryId { get; init; }

    public string CacheKey => $"{nameof(Region)}_{RegionId}";
    public int CacheExpiration => 5;
}
