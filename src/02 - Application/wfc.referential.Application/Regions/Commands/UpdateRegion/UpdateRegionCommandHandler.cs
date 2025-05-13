using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using BuildingBlocks.Infrastructure.CachingManagement;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.RegionAggregate.Exceptions;

namespace wfc.referential.Application.Regions.Commands.UpdateRegion;

public class PutRegionCommandHandler(IRegionRepository _regionRepository, ICacheService cacheService)
    : ICommandHandler<UpdateRegionCommand, Result<Guid>>
{

    public async Task<Result<Guid>> Handle(UpdateRegionCommand request, CancellationToken cancellationToken)
    {
        var region = await _regionRepository.GetByIdAsync(request.RegionId, cancellationToken);
        if (region is null) throw new ResourceNotFoundException("Region not found.");

        var hasDuplicatedCode = await _regionRepository.GetByCodeAsync(request.Code, cancellationToken);
        if (hasDuplicatedCode is not null) throw new CodeAlreadyExistException(request.Code);

        region.Update(request.Code, request.Name, request.IsEnabled, request.CountryId);

        await _regionRepository.UpdateRegionAsync(region, cancellationToken);

        await cacheService.SetAsync(request.CacheKey, region, TimeSpan.FromMinutes(request.CacheExpiration), cancellationToken);

        return Result.Success(region.Id!.Value);
    }
}
