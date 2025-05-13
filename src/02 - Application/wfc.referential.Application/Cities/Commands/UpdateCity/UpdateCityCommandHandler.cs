using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.RegionAggregate.Exceptions;

namespace wfc.referential.Application.Cities.Commands.UpdateCity;

public class UpdateCityCommandHandler(ICityRepository cityRepository, ICacheService _cacheService)
    : ICommandHandler<UpdateCityCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(UpdateCityCommand request, CancellationToken cancellationToken)
    {
        var city = await cityRepository.GetByIdAsync(request.CityId, cancellationToken);
        if (city is null) throw new ResourceNotFoundException($"{nameof(City)} not found.");

        var hasDuplicatedCode = await cityRepository.GetByCodeAsync(request.Code, cancellationToken);
        if (hasDuplicatedCode is not null) throw new CodeAlreadyExistException(request.Code);

        city.Update(request.Code, request.Name, request.Abbreviation, request.TimeZone, request.TaxZone, request.RegionId);

        await cityRepository.UpdateCityAsync(city, cancellationToken);

        await _cacheService.SetAsync(request.CacheKey,
            city,
            TimeSpan.FromMinutes(request.CacheExpiration),
            cancellationToken);


        return Result.Success(city.Id!.Value);
    }
}
