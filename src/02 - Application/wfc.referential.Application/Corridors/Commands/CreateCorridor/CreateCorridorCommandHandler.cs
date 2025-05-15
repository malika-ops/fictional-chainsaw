using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CorridorAggregate;

namespace wfc.referential.Application.Corridors.Commands.CreateCorridor;

public class CreateCorridorCommandHandler(ICorridorRepository corridorRepository, ICacheService cacheService) 
    : ICommandHandler<CreateCorridorCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateCorridorCommand request, CancellationToken cancellationToken)
    {
        request.CorridorId = CorridorId.Of(Guid.NewGuid());
        var corridor = Corridor.Create(request.CorridorId, request.SourceCountryId, request.DestinationCountryId,
            request.SourceCityId, request.DestinationCityId, request.SourceAgencyId, request.DestinationAgencyId);

        await corridorRepository.AddCorridorAsync(corridor, cancellationToken);

        await cacheService.SetAsync(request.CacheKey, corridor, TimeSpan.FromMinutes(request.CacheExpiration), cancellationToken);

        return Result.Success(corridor.Id!.Value);
    }
}
