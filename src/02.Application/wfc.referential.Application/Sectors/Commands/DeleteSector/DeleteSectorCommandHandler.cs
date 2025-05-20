using BuildingBlocks.Core.Abstraction.CQRS;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.SectorAggregate;
using wfc.referential.Domain.SectorAggregate.Exceptions;

namespace wfc.referential.Application.Sectors.Commands.DeleteSector;

public class DeleteSectorCommandHandler : ICommandHandler<DeleteSectorCommand, bool>
{
    private readonly ISectorRepository _sectorRepository;

    public DeleteSectorCommandHandler(ISectorRepository sectorRepository)
    {
        _sectorRepository = sectorRepository;
    }

    public async Task<bool> Handle(DeleteSectorCommand request, CancellationToken cancellationToken)
    {
        var sector = await _sectorRepository.GetByIdAsync(SectorId.Of(request.SectorId), cancellationToken);

        if (sector == null)
            throw new InvalidSectorDeletingException("Sector not found");

        // Check if sector has linked agencies
        var hasLinkedAgencies = await _sectorRepository.HasLinkedAgenciesAsync(sector.Id, cancellationToken);
        if (hasLinkedAgencies)
            throw new SectorLinkedToAgencyException(request.SectorId);

        // Disable the sector instead of deleting it
        sector.Disable();

        await _sectorRepository.UpdateSectorAsync(sector, cancellationToken);

        return true;
    }
}