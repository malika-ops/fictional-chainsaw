using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.SectorAggregate;
using wfc.referential.Domain.SectorAggregate.Exceptions;

namespace wfc.referential.Application.Sectors.Commands.DeleteSector;

public class DeleteSectorCommandHandler : ICommandHandler<DeleteSectorCommand, Result<bool>>
{
    private readonly ISectorRepository _sectorRepo;
    private readonly IAgencyRepository _agencyRepo;

    public DeleteSectorCommandHandler(ISectorRepository sectorRepo, IAgencyRepository agencyRepo)
    {
        _sectorRepo = sectorRepo;
        _agencyRepo = agencyRepo;
    }

    public async Task<Result<bool>> Handle(DeleteSectorCommand cmd, CancellationToken ct)
    {
        var sector = await _sectorRepo.GetByIdAsync(SectorId.Of(cmd.SectorId), ct);
        if (sector is null)
            throw new BusinessException($"Sector [{cmd.SectorId}] not found.");

        // Check if sector is linked to agencies using BaseRepository method
        var linkedAgencies = await _agencyRepo.GetByConditionAsync(a => a.SectorId == sector.Id, ct);
        if (linkedAgencies.Any())
            throw new SectorLinkedToAgencyException(cmd.SectorId);

        sector.Disable();
        await _sectorRepo.SaveChangesAsync(ct);
        return Result.Success(true);
    }
}