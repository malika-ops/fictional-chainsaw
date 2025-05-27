using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Sectors.Commands.DeleteSector;

public record DeleteSectorCommand : ICommand<Result<bool>>
{
    public Guid SectorId { get; }
    public DeleteSectorCommand(Guid sectorId) => SectorId = sectorId;
}
