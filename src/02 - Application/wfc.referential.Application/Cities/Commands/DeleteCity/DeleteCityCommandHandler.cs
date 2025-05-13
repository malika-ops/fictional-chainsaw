using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.RegionAggregate;

namespace wfc.referential.Application.Cities.Commands.DeleteCity;

public class DeleteCityCommandHandler(ICityRepository _cityRepository, ICacheService _cacheService) 
    : ICommandHandler<DeleteCityCommand, Result<bool>>
{
    

    public async Task<Result<bool>> Handle(DeleteCityCommand request, CancellationToken cancellationToken)
    {
        var city = await _cityRepository.GetByIdAsync(CityId.Of(request.CityId).Value, cancellationToken);

        if (city is null)
            throw new ResourceNotFoundException($"{nameof(City)} not found");

        city.SetInactive();
        await _cityRepository.UpdateCityAsync(city, cancellationToken);

        await _cacheService.RemoveAsync(request.CacheKey,
            cancellationToken);


        return Result.Success(true);
    }
}