using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Constants;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.CityAggregate.Exceptions;

namespace wfc.referential.Application.Cities.Commands.DeleteCity;

public class DeleteCityCommandHandler(ICityRepository _cityRepository, ICacheService _cacheService) 
    : ICommandHandler<DeleteCityCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteCityCommand request, CancellationToken cancellationToken)
    {
        var city = await _cityRepository.GetByIdAsync(CityId.Of(request.CityId).Value, cancellationToken);

        if (city is null)
            throw new ResourceNotFoundException($"{nameof(City)} with Id : {request.CityId} is not found");

        var cityHasAgency = await _cityRepository.HasAgencyAsync(CityId.Of(request.CityId), cancellationToken);
        if(cityHasAgency)
            throw new CityHasAgencyException(request.CityId);

        var cityHasSector = await _cityRepository.HasSectorAsync(CityId.Of(request.CityId), cancellationToken);
        if (cityHasSector)
            throw new CityHasSectorException(request.CityId);

        var cityHasCorridor = await _cityRepository.HasCorridorAsync(CityId.Of(request.CityId), cancellationToken);
        if (cityHasCorridor)
            throw new CityHasCorridorException(request.CityId);

        city.SetInactive();
        await _cityRepository.UpdateCityAsync(city, cancellationToken);

        await _cacheService.RemoveByPrefixAsync(CacheKeys.City.Prefix, cancellationToken);

        return Result.Success(true);
    }
}