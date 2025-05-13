using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.RegionAggregate;

namespace wfc.referential.Application.Cities.Commands.PatchCity;

public class PatchCityCommandHandler(ICityRepository cityRepository, ICacheService _cacheService)
    : ICommandHandler<PatchCityCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(PatchCityCommand request, CancellationToken cancellationToken)
    {
        var city = await cityRepository.GetByIdAsync(request.CityId, cancellationToken);

        if (city is null)
            throw new ResourceNotFoundException($"{nameof(Region)} not found");

        request.Adapt(city);
        city.Patch();
        await cityRepository.UpdateCityAsync(city, cancellationToken);

        await _cacheService.SetAsync(request.CacheKey,
            city,
            TimeSpan.FromMinutes(request.CacheExpiration),
            cancellationToken);


        return Result.Success(city.Id!.Value);
    }
}
