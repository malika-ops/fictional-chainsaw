using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Caching.Interface;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.RegionAggregate;

namespace wfc.referential.Application.Regions.Commands.CreateRegion;

public record CreateRegionCommand : ICommand<Result<Guid>>
{
    public RegionId RegionId { get; set; } = default!;
    public string Code { get; init; } = default!;
    public string Name { get; init; } = default!;
    public CountryId CountryId { get; init; } = default!;

}