using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CorridorAggregate;

namespace wfc.referential.Application.Corridors.Commands.PatchCorridor;

public class PatchCorridorCommandHandler(ICorridorRepository _regionRepository, ICacheService cacheService) 
    : ICommandHandler<PatchCorridorCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(PatchCorridorCommand request, CancellationToken cancellationToken)
    {
        var corridor = await _regionRepository.GetByIdAsync(request.CorridorId, cancellationToken);
        if (corridor is null)
            throw new ResourceNotFoundException($"{nameof(Corridor)} not found");

        request.Adapt(corridor);
        corridor.Patch();
        await _regionRepository.UpdateCorridorAsync(corridor, cancellationToken);

        await cacheService.SetAsync(request.CacheKey, corridor, TimeSpan.FromMinutes(request.CacheExpiration), cancellationToken);

        return Result.Success(corridor.Id!.Value);
    }
}
