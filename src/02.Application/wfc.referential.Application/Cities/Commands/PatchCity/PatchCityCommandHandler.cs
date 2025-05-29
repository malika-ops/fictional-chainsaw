using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.RegionAggregate;

namespace wfc.referential.Application.Cities.Commands.PatchCity;

public class PatchCityCommandHandler(ICityRepository cityRepository,
    IRegionRepository _regionRepository)
    : ICommandHandler<PatchCityCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(PatchCityCommand request, CancellationToken cancellationToken)
    {
        var cityId = CityId.Of(request.CityId);
        var city = await cityRepository.GetOneByConditionAsync(c => c.Id == cityId, cancellationToken);

        if (city is null)
        {
            throw new ResourceNotFoundException($"{nameof(City)} with Id : {request.CityId} is not found");
        }
        var regionId = request.RegionId;
        if(regionId != null)
        {
            var updatedRegion = await _regionRepository.GetOneByConditionAsync(r => r.Id == regionId, cancellationToken);
            if(updatedRegion is null)
                throw new ResourceNotFoundException($"{nameof(Region)} with Id : {regionId} is not found");
        }
        city.Patch(request.Code, request.Name, request.Abbreviation, request.TimeZone, request.IsEnabled, regionId);

        cityRepository.Update(city);
        await cityRepository.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}
