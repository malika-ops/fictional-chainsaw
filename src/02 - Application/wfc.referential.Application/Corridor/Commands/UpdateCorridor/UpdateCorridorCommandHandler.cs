using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.RegionAggregate.Exceptions;

namespace wfc.referential.Application.Corridors.Commands.UpdateCorridor;

public class UpdateCorridorCommandHandler(ICorridorRepository _corridorRepository, ICacheService cacheService)
    : ICommandHandler<UpdateCorridorCommand, Result<Guid>>
{

    public async Task<Result<Guid>> Handle(UpdateCorridorCommand request, CancellationToken cancellationToken)
    {
        var corridor = await _corridorRepository.GetByIdAsync(request.CorridorId, cancellationToken);
        if (corridor is null) throw new ResourceNotFoundException("Corridor not found.");


        corridor.Update(request.SourceCountryId, request.DestinationCountryId,
            request.SourceCityId, request.DestinationCityId, request.SourceAgencyId,
            request.DestinationAgencyId, request.IsEnabled);

        await _corridorRepository.UpdateCorridorAsync(corridor, cancellationToken);

        await cacheService.SetAsync(request.CacheKey, corridor, TimeSpan.FromMinutes(request.CacheExpiration), cancellationToken);

        return Result.Success(corridor.Id!.Value);
    }
}
