using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.RegionAggregate;
using wfc.referential.Domain.RegionAggregate.Exceptions;

namespace wfc.referential.Application.Regions.Commands.DeleteRegion;

public class DeleteRegionCommandHandler(IRegionRepository _regionRepository, ICacheService cacheService) 
    : ICommandHandler<DeleteRegionCommand, Result<bool>>
{
    

    public async Task<Result<bool>> Handle(DeleteRegionCommand request, CancellationToken cancellationToken)
    {
        var region = await _regionRepository.GetByIdAsync(RegionId.Of(request.RegionId).Value, cancellationToken);

        if (region is null)
            throw new ResourceNotFoundException($"{nameof(Region)} not found");

        var cities = await _regionRepository.GetCitiesByRegionIdAsync(region.Id!.Value, cancellationToken);

        if (cities.Count > 0) throw new RegionHasCitiesException(cities);
        region.SetInactive();
        await _regionRepository.UpdateRegionAsync(region, cancellationToken);

        await cacheService.RemoveAsync(request.CacheKey, cancellationToken);

        return Result.Success(true);
    }
}