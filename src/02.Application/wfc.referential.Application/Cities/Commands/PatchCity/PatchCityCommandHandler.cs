using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using Mapster;
using wfc.referential.Application.Constants;
using Microsoft.Extensions.Logging;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.RegionAggregate;

namespace wfc.referential.Application.Cities.Commands.PatchCity;

public class PatchCityCommandHandler(ICityRepository cityRepository,
    IRegionRepository _regionRepository,
    ILogger<PatchCityCommandHandler> logger,
    ICacheService _cacheService)
    : ICommandHandler<PatchCityCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(PatchCityCommand request, CancellationToken cancellationToken)
    {
        var city = await cityRepository.GetByIdAsync(request.CityId, cancellationToken);

        if (city is null)
        {
            logger.LogWarning($"{nameof(City)} not found");
            throw new ResourceNotFoundException($"{nameof(City)} with Id : {request.CityId} is not found");
        }

        if(request.RegionId != null)
        {
            var updatedRegion = await _regionRepository.GetByIdAsync(request.RegionId!.Value, cancellationToken);
            if(updatedRegion is null)
                throw new ResourceNotFoundException($"{nameof(Region)} with Id : {request.RegionId} is not found");
        }

        request.Adapt(city);
        city.Patch();
        await cityRepository.UpdateCityAsync(city, cancellationToken);

        await _cacheService.RemoveByPrefixAsync(CacheKeys.City.Prefix, cancellationToken);

        return Result.Success(city.Id!.Value);
    }
}
