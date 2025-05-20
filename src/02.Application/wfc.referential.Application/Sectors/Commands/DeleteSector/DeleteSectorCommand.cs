using BuildingBlocks.Core.Abstraction.CQRS;

namespace wfc.referential.Application.Sectors.Commands.DeleteSector;

public class DeleteSectorCommand : ICommand<bool>
{
    public Guid SectorId { get; set; }

    public DeleteSectorCommand(Guid sectorId)
    {
        SectorId = sectorId;
    }
}