using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.RegionAggregate;
using wfc.referential.Domain.RegionAggregate.Exceptions;

namespace wfc.referential.Application.Regions.Commands.PatchRegion;

public class PatchRegionCommandHandler(IRegionRepository _regionRepository, ICacheService cacheService) 
    : ICommandHandler<PatchRegionCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(PatchRegionCommand request, CancellationToken cancellationToken)
    {
        var region = await _regionRepository.GetByIdAsync(request.RegionId, cancellationToken);
        if (region is null)
            throw new ResourceNotFoundException($"{nameof(Region)} not found");

        var codeAlreadyExist = await _regionRepository.GetByCodeAsync(request.Code, cancellationToken);
        if (codeAlreadyExist is not null)
            throw new CodeAlreadyExistException(codeAlreadyExist.Code);

        request.Adapt(region);
        region.Patch();
        await _regionRepository.UpdateRegionAsync(region, cancellationToken);

        await cacheService.SetAsync(request.CacheKey, region, TimeSpan.FromMinutes(request.CacheExpiration), cancellationToken);

        return Result.Success(region.Id!.Value);
    }
}
