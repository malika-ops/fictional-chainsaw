using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.RegionAggregate;
using wfc.referential.Domain.RegionAggregate.Exceptions;

namespace wfc.referential.Application.Regions.Commands.UpdateRegion;

public class PutRegionCommandHandler(IRegionRepository _regionRepository)
    : ICommandHandler<UpdateRegionCommand, Result<bool>>
{

    public async Task<Result<bool>> Handle(UpdateRegionCommand request, CancellationToken cancellationToken)
    {
        var region = await _regionRepository.GetOneByConditionAsync(r => r.Id == RegionId.Of(request.RegionId), cancellationToken);
        if (region is null) throw new ResourceNotFoundException("Region not found.");

        var hasDuplicatedCode = await _regionRepository.GetOneByConditionAsync(r => r.Code == request.Code, cancellationToken);
        if (hasDuplicatedCode is not null) throw new CodeAlreadyExistException(request.Code);

        region.Update(request.Code, request.Name, request.IsEnabled, request.CountryId);

        _regionRepository.Update(region);
        await _regionRepository.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}
