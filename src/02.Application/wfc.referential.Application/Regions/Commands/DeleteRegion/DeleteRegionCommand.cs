using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Regions.Commands.DeleteRegion;

public record DeleteRegionCommand : ICommand<Result<bool>>
{
    public Guid RegionId { get; init; }
}