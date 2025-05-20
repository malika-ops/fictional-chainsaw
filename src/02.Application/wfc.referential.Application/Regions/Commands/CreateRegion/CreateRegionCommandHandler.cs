using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.RegionAggregate;
using wfc.referential.Domain.RegionAggregate.Exceptions;

namespace wfc.referential.Application.Regions.Commands.CreateRegion;

public class CreateRegionCommandHandler(IRegionRepository regionRepository, ICacheService cacheService) 
    : ICommandHandler<CreateRegionCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateRegionCommand request, CancellationToken cancellationToken)
    {
        var isExist = await regionRepository.GetByCodeAsync(request.Code,cancellationToken);
        if (isExist is not null) throw new CodeAlreadyExistException(request.Code);

        request.RegionId = RegionId.Of(Guid.NewGuid());
        var region = Region.Create(request.RegionId, request.Code, request.Name, request.CountryId);

        await regionRepository.AddRegionAsync(region, cancellationToken);

        await cacheService.SetAsync(request.CacheKey, region, TimeSpan.FromMinutes(request.CacheExpiration), cancellationToken);

        return Result.Success(region.Id!.Value);
    }
}
