using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Constants;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.RegionAggregate;
using wfc.referential.Domain.RegionAggregate.Exceptions;

namespace wfc.referential.Application.Regions.Commands.DeleteRegion;

public class DeleteRegionCommandHandler(IRegionRepository _regionRepository,
    ICityRepository _cityRepository, ICacheService _cacheService) 
    : ICommandHandler<DeleteRegionCommand, Result<bool>>
{
    

    public async Task<Result<bool>> Handle(DeleteRegionCommand request, CancellationToken cancellationToken)
    {
        var regionId = RegionId.Of(request.RegionId);
        var region = await _regionRepository.GetOneByConditionAsync(r => r.Id == regionId, cancellationToken);

        if (region is null)
            throw new ResourceNotFoundException($"{nameof(Region)} not found");

        var cities = await _cityRepository.GetByConditionAsync(c => c.RegionId == region.Id, cancellationToken);
        if (cities.Any()) throw new RegionHasCitiesException(cities);

        region.SetInactive();

        _regionRepository.Update(region);
        await _regionRepository.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveByPrefixAsync(CacheKeys.Region.Prefix, cancellationToken);

        return Result.Success(true);
    }
}