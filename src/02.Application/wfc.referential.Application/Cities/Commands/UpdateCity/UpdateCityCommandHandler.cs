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
    IRegionRepository _regionRepository)
    : ICommandHandler<UpdateCityCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(UpdateCityCommand request, CancellationToken cancellationToken)
    {
        var city = await cityRepository.GetOneByConditionAsync(c => c.Id == CityId.Of(request.CityId), cancellationToken);
        if (city is null) throw new ResourceNotFoundException($"{nameof(City)} with Id : {request.CityId} is not found");

        var regionId = request.RegionId;
        if (regionId != null)
        {
            var updatedRegion = await _regionRepository.GetOneByConditionAsync(r => r.Id == regionId, cancellationToken);
            if (updatedRegion is null)
                throw new ResourceNotFoundException($"{nameof(Region)} with Id : {regionId} is not found");
        }

        var hasDuplicatedCode = await cityRepository.GetOneByConditionAsync(c => c.Code == request.Code, cancellationToken);
        if (hasDuplicatedCode is not null) throw new CodeAlreadyExistException(request.Code);

        city.Update(request.Code, request.Name, request.Abbreviation, request.TimeZone, regionId!);

        cityRepository.Update(city);
        await cityRepository.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}
