using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.CityAggregate.Exceptions;

namespace wfc.referential.Application.Cities.Commands.DeleteCity;

public class DeleteCityCommandHandler(ICityRepository _cityRepository,
    IAgencyRepository _agencyRepository, 
    ISectorRepository _sectorReository,
    ICorridorRepository _corridorRepository)
    : ICommandHandler<DeleteCityCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteCityCommand request, CancellationToken cancellationToken)
    {
        var cityId = CityId.Of(request.CityId);
        var city = await _cityRepository.GetByIdAsync(cityId, cancellationToken);

        if (city is null)
            throw new ResourceNotFoundException($"{nameof(City)} with Id : {request.CityId} is not found");

        var cityHasAgency = await _agencyRepository.GetByConditionAsync(a => a.CityId == cityId, cancellationToken);
        if (cityHasAgency.Any())
            throw new CityHasAgencyException(request.CityId);

        var cityHasSector = await _sectorReository.GetByConditionAsync(s => s.CityId == cityId, cancellationToken);
        if (cityHasSector.Any())
            throw new CityHasSectorException(request.CityId);

        var cityHasCorridor = await _corridorRepository.GetByConditionAsync(c=>c.SourceCityId == cityId || c.DestinationCityId == cityId, cancellationToken);
        if (cityHasCorridor.Any())
            throw new CityHasCorridorException(request.CityId);

        city.SetInactive();
        _cityRepository.Update(city);
        await _cityRepository.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}