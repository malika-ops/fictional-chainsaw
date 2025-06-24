using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Regions.Commands.CreateRegion;

public record CreateRegionCommand : ICommand<Result<Guid>>
{
    public string Code { get; init; } = default!;
    public string Name { get; init; } = default!;
    public Guid CountryId { get; init; } = default!;

}