using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Sectors.Commands.PatchSector;

public record PatchSectorCommand : ICommand<Result<bool>>
{
    public Guid SectorId { get; init; }
    public string? Code { get; init; }
    public string? Name { get; init; }
    public Guid? CityId { get; init; }
    public bool? IsEnabled { get; init; }
}