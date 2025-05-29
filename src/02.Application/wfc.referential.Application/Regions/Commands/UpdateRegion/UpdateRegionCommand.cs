using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Caching.Interface;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.RegionAggregate;

namespace wfc.referential.Application.Regions.Commands.UpdateRegion;

public record UpdateRegionCommand : ICommand<Result<bool>>
{
    public Guid RegionId { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public bool IsEnabled { get; set; }
    public CountryId CountryId { get; set; }
}
