using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CorridorAggregate;

namespace wfc.referential.Application.Corridors.Commands.DeleteCorridor;

public class DeleteCorridorCommandHandler(ICorridorRepository _corridorRepository, ICacheService cacheService) 
    : ICommandHandler<DeleteCorridorCommand, Result<bool>>
{
    

    public async Task<Result<bool>> Handle(DeleteCorridorCommand request, CancellationToken cancellationToken)
    {
        var corridor = await _corridorRepository.GetByIdAsync(CorridorId.Of(request.CorridorId).Value, cancellationToken);

        if (corridor is null)
            throw new ResourceNotFoundException($"{nameof(Corridor)} not found");

        corridor.SetInactive();
        await _corridorRepository.UpdateCorridorAsync(corridor, cancellationToken);

        await cacheService.RemoveAsync(request.CacheKey, cancellationToken);

        return Result.Success(true);
    }
}