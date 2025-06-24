using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Regions.Commands.PatchRegion;

public record PatchRegionCommand : ICommand<Result<bool>>
{
    // The ID from the route
    public Guid RegionId { get; init; }

    // The optional fields to update
    public string? Code { get; init; }
    public string? Name { get; init; }
    public bool? IsEnabled { get; init; }
    public Guid? CountryId { get; init; }
}
