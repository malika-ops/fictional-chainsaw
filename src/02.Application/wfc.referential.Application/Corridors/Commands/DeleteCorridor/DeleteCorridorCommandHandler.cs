using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Constants;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CorridorAggregate;

namespace wfc.referential.Application.Corridors.Commands.DeleteCorridor;

public class DeleteCorridorCommandHandler(ICorridorRepository _corridorRepository, ICacheService _cacheService) 
    : ICommandHandler<DeleteCorridorCommand, Result<bool>>
{
    

    public async Task<Result<bool>> Handle(DeleteCorridorCommand request, CancellationToken cancellationToken)
    {
        var corridorId = CorridorId.Of(request.CorridorId);
        var corridor = await _corridorRepository.GetOneByConditionAsync(c => c.Id == corridorId, cancellationToken);

        if (corridor is null)
            throw new ResourceNotFoundException($"{nameof(Corridor)} not found");

        corridor.SetInactive();
        _corridorRepository.Update(corridor);
        await _corridorRepository.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveByPrefixAsync(CacheKeys.Corridor.Prefix, cancellationToken);

        return Result.Success(true);
    }
}