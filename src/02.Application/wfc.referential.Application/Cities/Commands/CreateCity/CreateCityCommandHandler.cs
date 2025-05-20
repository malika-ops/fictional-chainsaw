using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Application.Constants;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.CityAggregate.Exceptions;
using wfc.referential.Domain.RegionAggregate;

namespace wfc.referential.Application.Cities.Commands.CreateCity;

public class CreateCityCommandHandler(ICityRepository cityRepository,
    IRegionRepository _regionRepository,
    ICacheService _cacheService)
    : ICommandHandler<CreateCityCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateCityCommand request, CancellationToken cancellationToken)
    {
        var isExist = await cityRepository.GetByCodeAsync(request.CityCode, cancellationToken);
        if (isExist is not null) throw new CodeAlreadyExistException(request.CityCode);


        if (request.RegionId != Guid.Empty)
        {
            var updatedRegion = await _regionRepository.GetByIdAsync(request.RegionId, cancellationToken);
            if (updatedRegion is null)
                throw new ResourceNotFoundException($"{nameof(Region)} with Id : {request.RegionId} is not found");
        }

        request.CityId = CityId.Create();
        var city = City.Create(request.CityId, request.CityCode, request.CityName, request.TimeZone, RegionId.Of(request.RegionId), request.Abbreviation);

        await cityRepository.AddCityAsync(city, cancellationToken);

        await _cacheService.RemoveByPrefixAsync(CacheKeys.City.Prefix,cancellationToken);

        return Result.Success(city.Id!.Value);
    }
}