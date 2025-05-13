using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Infrastructure.CachingManagement;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.CityAggregate.Exceptions;

namespace wfc.referential.Application.Cities.Commands.CreateCity;

public class CreateCityCommandHandler(ICityRepository cityRepository, ICacheService _cacheService)
    : ICommandHandler<CreateCityCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateCityCommand request, CancellationToken cancellationToken)
    {
        var isExist = await cityRepository.GetByCodeAsync(request.CityCode, cancellationToken);
        if (isExist is not null) throw new CodeAlreadyExistException(request.CityCode);

        request.CityId = CityId.Create();
        var city = City.Create(request.CityId, request.CityCode, request.CityName, request.TimeZone, request.TaxZone, request.RegionId, request.Abbreviation);

        await cityRepository.AddCityAsync(city, cancellationToken);

        await _cacheService.SetAsync(request.CacheKey,
            city,
            TimeSpan.FromMinutes(request.CacheExpiration),
            cancellationToken);


        return Result.Success(city.Id!.Value);
    }
}