using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Application.Constants;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CorridorAggregate;

namespace wfc.referential.Application.Corridors.Commands.CreateCorridor;

public class CreateCorridorCommandHandler(ICorridorRepository corridorRepository, ICacheService _cacheService) 
    : ICommandHandler<CreateCorridorCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateCorridorCommand request, CancellationToken cancellationToken)
    {
        request.CorridorId = CorridorId.Of(Guid.NewGuid());
        var corridor = Corridor.Create(request.CorridorId, request.SourceCountryId, request.DestinationCountryId,
            request.SourceCityId, request.DestinationCityId, request.SourceBranchId, request.DestinationBranchId);

        await corridorRepository.AddAsync(corridor, cancellationToken);
        await corridorRepository.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveByPrefixAsync(CacheKeys.Corridor.Prefix, cancellationToken);

        return Result.Success(corridor.Id!.Value);
    }
}
