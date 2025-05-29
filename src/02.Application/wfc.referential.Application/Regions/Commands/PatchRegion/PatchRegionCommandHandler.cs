using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Constants;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.RegionAggregate;
using wfc.referential.Domain.RegionAggregate.Exceptions;

namespace wfc.referential.Application.Regions.Commands.PatchRegion;

public class PatchRegionCommandHandler(IRegionRepository _regionRepository, ICacheService _cacheService) 
    : ICommandHandler<PatchRegionCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(PatchRegionCommand request, CancellationToken cancellationToken)
    {
        var region = await _regionRepository.GetOneByConditionAsync(r => r.Id == RegionId.Of(request.RegionId), cancellationToken);
        if (region is null)
            throw new ResourceNotFoundException($"{nameof(Region)} not found");

        var codeAlreadyExist = await _regionRepository.GetOneByConditionAsync(r => r.Code.Equals(request.Code), cancellationToken);
        if (codeAlreadyExist is not null)
            throw new CodeAlreadyExistException(codeAlreadyExist.Code);

        region.Patch(request.Code, request.Name, request.IsEnabled, request.CountryId);

         _regionRepository.Update(region);
        await _regionRepository.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveByPrefixAsync(CacheKeys.Region.Prefix, cancellationToken);

        return Result.Success(true);
    }
}
