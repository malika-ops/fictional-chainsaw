using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.SectorAggregate;
using wfc.referential.Domain.SectorAggregate.Exceptions;

namespace wfc.referential.Application.Sectors.Commands.PatchSector;

public class PatchSectorCommandHandler : ICommandHandler<PatchSectorCommand, Result<bool>>
{
    private readonly ISectorRepository _sectorRepo;
    private readonly ICityRepository _cityRepo;

    public PatchSectorCommandHandler(ISectorRepository sectorRepo, ICityRepository cityRepo)
    {
        _sectorRepo = sectorRepo;
        _cityRepo = cityRepo;
    }

    public async Task<Result<bool>> Handle(PatchSectorCommand cmd, CancellationToken ct)
    {
        // Check if sector exists
        var sector = await _sectorRepo.GetByIdAsync(SectorId.Of(cmd.SectorId), ct);
        if (sector is null)
            throw new BusinessException($"Sector not found");

        // Validate city exists if CityId is provided
        if (cmd.CityId.HasValue)
        {
            var city = await _cityRepo.GetByIdAsync(CityId.Of(cmd.CityId.Value), ct);
            if (city is null)
                throw new BusinessException($"City with ID {cmd.CityId.Value} not found");
        }

        // Check for duplicate code if Code is being updated
        if (!string.IsNullOrWhiteSpace(cmd.Code))
        {
            var duplicate = await _sectorRepo.GetOneByConditionAsync(s => s.Code == cmd.Code, ct);
            if (duplicate is not null && duplicate.Id != sector.Id)
                throw new SectorCodeAlreadyExistException(cmd.Code);
        }

        // Apply the patch
        sector.Patch(
            cmd.Code,
            cmd.Name,
            cmd.CityId.HasValue ? CityId.Of(cmd.CityId.Value) : null,
            cmd.IsEnabled);

        _sectorRepo.Update(sector);
        await _sectorRepo.SaveChangesAsync(ct);

        return Result.Success(true);
    }
}