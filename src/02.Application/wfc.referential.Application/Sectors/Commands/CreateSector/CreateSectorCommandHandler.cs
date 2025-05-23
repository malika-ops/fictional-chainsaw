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

    public CreateSectorCommandHandler(
        ISectorRepository sectorRepository,
        ICityRepository cityRepository)
    {
        _sectorRepository = sectorRepository;
        _cityRepository = cityRepository;
    }

    public async Task<Result<Guid>> Handle(CreateSectorCommand request, CancellationToken cancellationToken)
    {
        // Check if the code already exists
        var existingCode = await _sectorRepository.GetByCodeAsync(request.Code, cancellationToken);
        if (existingCode is not null)
            throw new SectorCodeAlreadyExistException(request.Code);

        // Check if the City exist
        var city = await _cityRepository.GetByIdAsync(CityId.Of(request.CityId), cancellationToken);
        if (city is null)
            throw new BusinessException($"City with ID {request.CityId} not found");

        var id = SectorId.Of(Guid.NewGuid());
        var sector = Sector.Create(id, request.Code, request.Name, city);

        await _sectorRepository.AddSectorAsync(sector, cancellationToken);

        return Result.Success(sector.Id.Value);
    }
}