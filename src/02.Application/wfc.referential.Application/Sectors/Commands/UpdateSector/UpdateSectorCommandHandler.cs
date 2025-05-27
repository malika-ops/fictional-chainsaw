using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.SectorAggregate;
using wfc.referential.Domain.SectorAggregate.Exceptions;

namespace wfc.referential.Application.Sectors.Commands.UpdateSector;

public class UpdateSectorCommandHandler
    : ICommandHandler<UpdateSectorCommand, Result<bool>>
{
    private readonly ISectorRepository _sectorRepo;
    private readonly ICityRepository _cityRepo;

    public UpdateSectorCommandHandler(ISectorRepository sectorRepo, ICityRepository cityRepo)
    {
        _sectorRepo = sectorRepo;
        _cityRepo = cityRepo;
    }

    public async Task<Result<bool>> Handle(UpdateSectorCommand cmd, CancellationToken ct)
    {
        // Check if sector exists
        var sector = await _sectorRepo.GetByIdAsync(SectorId.Of(cmd.SectorId), ct);
        if (sector is null)
            throw new BusinessException($"Sector [{cmd.SectorId}] not found.");

        // Check if city exists
        var city = await _cityRepo.GetByIdAsync(CityId.Of(cmd.CityId), ct);
        if (city is null)
            throw new BusinessException($"City with ID {cmd.CityId} not found");

        // Check uniqueness on Code (exclude current sector)
        var duplicate = await _sectorRepo.GetOneByConditionAsync(s => s.Code == cmd.Code, ct);
        if (duplicate is not null && duplicate.Id != sector.Id)
            throw new SectorCodeAlreadyExistException(cmd.Code);

        // Update the sector
        sector.Update(
            cmd.Code,
            cmd.Name,
            CityId.Of(cmd.CityId),
            cmd.IsEnabled);

        _sectorRepo.Update(sector);
        await _sectorRepo.SaveChangesAsync(ct);
        return Result.Success(true);
    }
}