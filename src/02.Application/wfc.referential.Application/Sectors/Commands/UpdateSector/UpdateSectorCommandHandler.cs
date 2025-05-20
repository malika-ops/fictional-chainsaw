using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.SectorAggregate;
using wfc.referential.Domain.SectorAggregate.Exceptions;

namespace wfc.referential.Application.Sectors.Commands.UpdateSector;

public class UpdateSectorCommandHandler : ICommandHandler<UpdateSectorCommand, Guid>
{
    private readonly ISectorRepository _sectorRepository;
    private readonly ICityRepository _cityRepository;

    public UpdateSectorCommandHandler(
        ISectorRepository sectorRepository,
        ICityRepository cityRepository)
    {
        _sectorRepository = sectorRepository;
        _cityRepository = cityRepository;
    }

    public async Task<Guid> Handle(UpdateSectorCommand request, CancellationToken cancellationToken)
    {
        // Check if sector exists
        var sector = await _sectorRepository.GetByIdAsync(new SectorId(request.SectorId), cancellationToken);
        if (sector is null)
            throw new BusinessException($"Sector with ID {request.SectorId} not found");

        // Check if code is unique (if changed)
        var existingWithCode = await _sectorRepository.GetByCodeAsync(request.Code, cancellationToken);
        if (existingWithCode is not null && existingWithCode.Id.Value != request.SectorId)
            throw new SectorCodeAlreadyExistException(request.Code);

        var city = await _cityRepository.GetByIdAsync(request.CityId, cancellationToken);
        if (city is null)
            throw new BusinessException($"City with ID {request.CityId} not found");

        // Update the sector
        sector.Update(request.Code, request.Name, city);

        // Update enabled status if necessary
        if (request.IsEnabled && !sector.IsEnabled)
        {
            sector.Activate();
        }
        else if (!request.IsEnabled && sector.IsEnabled)
        {
            sector.Disable();
        }

        await _sectorRepository.UpdateSectorAsync(sector, cancellationToken);

        return sector.Id.Value;
    }
}