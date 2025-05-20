using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using Microsoft.Extensions.Logging;
using wfc.referential.Application.Constants;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.RegionAggregate;
using wfc.referential.Domain.RegionAggregate.Exceptions;

namespace wfc.referential.Application.Cities.Commands.UpdateCity;

public class UpdateCityCommandHandler(ICityRepository cityRepository,
    IRegionRepository _regionRepository,
    ICacheService _cacheService)
    : ICommandHandler<UpdateCityCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(UpdateCityCommand request, CancellationToken cancellationToken)
    {
        var city = await cityRepository.GetByIdAsync(request.CityId, cancellationToken);
        if (city is null) throw new ResourceNotFoundException($"{nameof(City)} with Id : {request.CityId} is not found");

        if (request.RegionId != null)
        {
            var updatedRegion = await _regionRepository.GetByIdAsync(request.RegionId!.Value, cancellationToken);
            if (updatedRegion is null)
                throw new ResourceNotFoundException($"{nameof(Region)} with Id : {request.RegionId} is not found");
        }

        var hasDuplicatedCode = await cityRepository.GetByCodeAsync(request.Code, cancellationToken);
        if (hasDuplicatedCode is not null) throw new CodeAlreadyExistException(request.Code);

        city.Update(request.Code, request.Name, request.Abbreviation, request.TimeZone, request.RegionId);

        await cityRepository.UpdateCityAsync(city, cancellationToken);

        await _cacheService.RemoveByPrefixAsync(CacheKeys.City.Prefix, cancellationToken);

        return Result.Success(city.Id!.Value);
    }
}
