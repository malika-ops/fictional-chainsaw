using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.SectorAggregate;
using wfc.referential.Domain.SectorAggregate.Exceptions;

namespace wfc.referential.Application.Sectors.Commands.CreateSector;

public class CreateSectorCommandHandler : ICommandHandler<CreateSectorCommand, Result<Guid>>
{
    private readonly ISectorRepository _sectorRepository;
    private readonly ICityRepository _cityRepository;

    public CreateSectorCommandHandler(ISectorRepository sectorRepository, ICityRepository cityRepository)
    {
        _sectorRepository = sectorRepository;
        _cityRepository = cityRepository;
    }

    public async Task<Result<Guid>> Handle(CreateSectorCommand command, CancellationToken ct)
    {
        // Check if city exists first
        var city = await _cityRepository.GetByIdAsync(CityId.Of(command.CityId), ct);
        if (city is null)
            throw new BusinessException($"City with ID {command.CityId} not found");

        // Check if sector code already exists
        var existingSector = await _sectorRepository.GetOneByConditionAsync(s => s.Code == command.Code, ct);
        if (existingSector is not null)
            throw new SectorCodeAlreadyExistException(command.Code);

        // Create the sector
        var sector = Sector.Create(
            SectorId.Of(Guid.NewGuid()),
            command.Code,
            command.Name,
            CityId.Of(command.CityId));

        await _sectorRepository.AddAsync(sector, ct);
        await _sectorRepository.SaveChangesAsync(ct);
        return Result.Success(sector.Id!.Value);
    }
}