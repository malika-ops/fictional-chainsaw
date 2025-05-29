using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.RegionAggregate;

namespace wfc.referential.Application.Cities.Commands.PatchCity;

public record PatchCityCommand : ICommand<Result<bool>>
{
    // The ID from the route
    public Guid CityId { get; init; }
    public string? Code { get; init; }
    public string? Name { get; init; }
    public string? Abbreviation { get; init; }
    public string? TimeZone { get; init; }
    public bool? IsEnabled { get; init; }
    public RegionId? RegionId { get; init; }
}
