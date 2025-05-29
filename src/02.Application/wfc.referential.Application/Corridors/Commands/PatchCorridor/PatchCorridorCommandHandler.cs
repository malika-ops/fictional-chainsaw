using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Constants;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CorridorAggregate;

namespace wfc.referential.Application.Corridors.Commands.PatchCorridor;

public class PatchCorridorCommandHandler(ICorridorRepository _regionRepository, ICacheService _cacheService) 
    : ICommandHandler<PatchCorridorCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(PatchCorridorCommand request, CancellationToken cancellationToken)
    {
        var corridorId = CorridorId.Of(request.CorridorId);
        var corridor = await _regionRepository.GetOneByConditionAsync(c => c.Id == corridorId, cancellationToken);
        if (corridor is null)
            throw new ResourceNotFoundException($"{nameof(Corridor)} not found");

        corridor.Patch(request.SourceCountryId, request.DestinationCountryId,
            request.SourceCityId, request.DestinationCityId,
            request.SourceBranchId, request.DestinationBranchId, request.IsEnabled);

        _regionRepository.Update(corridor);
        await _regionRepository.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveByPrefixAsync(CacheKeys.Corridor.Prefix, cancellationToken);

        return Result.Success(true);
    }
}
