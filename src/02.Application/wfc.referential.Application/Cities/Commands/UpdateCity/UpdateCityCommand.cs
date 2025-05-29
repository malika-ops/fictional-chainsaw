using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.RegionAggregate;

namespace wfc.referential.Application.Cities.Commands.UpdateCity;

public record UpdateCityCommand : ICommand<Result<bool>>
{
    public Guid CityId { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Abbreviation { get; set; } = default!;
    public string TimeZone { get;  set; } = default!;
    public bool IsEnabled { get; set; }
    public RegionId? RegionId { get; set; } = default!;
}
