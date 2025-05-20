using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.SectorAggregate;
using wfc.referential.Domain.SectorAggregate.Exceptions;

namespace wfc.referential.Application.Sectors.Commands.PatchSector;

public class PatchSectorCommandHandler : ICommandHandler<PatchSectorCommand, Guid>
{
    private readonly ISectorRepository _sectorRepository;
    private readonly ICityRepository _cityRepository;

    public PatchSectorCommandHandler(
        ISectorRepository sectorRepository,
        ICityRepository cityRepository)
    {
        _sectorRepository = sectorRepository;
        _cityRepository = cityRepository;
    }

    public async Task<Guid> Handle(PatchSectorCommand request, CancellationToken cancellationToken)
    {
        var sector = await _sectorRepository.GetByIdAsync(new SectorId(request.SectorId), cancellationToken);
        if (sector == null)
            throw new BusinessException("Sector not found");

        // Check if code is unique if it's being updated
        if (request.Code != null && request.Code != sector.Code)
        {
            var existingWithCode = await _sectorRepository.GetByCodeAsync(request.Code, cancellationToken);
            if (existingWithCode != null && existingWithCode.Id.Value != request.SectorId)
                throw new SectorCodeAlreadyExistException(request.Code);
        }

        // Collect updates for domain entities
        var updatedCode = request.Code ?? sector.Code;
        var updatedName = request.Name ?? sector.Name;

        // Get city if it's being updated, otherwise use existing
        var city = sector.City;
        if (request.CityId.HasValue)
        {
            var updatedCity = await _cityRepository.GetByIdAsync(request.CityId.Value, cancellationToken);
            if (updatedCity == null)
                throw new BusinessException($"City with ID {request.CityId} not found");
            city = updatedCity;
        }

        // Update via domain methods instead of property setters
        sector.Update(updatedCode, updatedName, city);

        // Additionally call patch to raise the event
        sector.Patch();

        // Handle enabled status if provided
        if (request.IsEnabled.HasValue)
        {
            if (request.IsEnabled.Value && !sector.IsEnabled)
            {
                sector.Activate();
            }
            else if (!request.IsEnabled.Value && sector.IsEnabled)
            {
                sector.Disable();
            }
        }

        await _sectorRepository.UpdateSectorAsync(sector, cancellationToken);

        return sector.Id.Value;
    }
}