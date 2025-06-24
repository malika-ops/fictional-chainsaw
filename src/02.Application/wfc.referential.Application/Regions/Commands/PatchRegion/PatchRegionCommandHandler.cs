using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.RegionAggregate;
using wfc.referential.Domain.RegionAggregate.Exceptions;

namespace wfc.referential.Application.Regions.Commands.PatchRegion;

public class PatchRegionCommandHandler(IRegionRepository _regionRepository, ICountryRepository countryRepository) 
    : ICommandHandler<PatchRegionCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(PatchRegionCommand request, CancellationToken cancellationToken)
    {
        var regionId = RegionId.Of(request.RegionId);
        var region = await _regionRepository.GetOneByConditionAsync(r => r.Id == regionId, cancellationToken);
        if (region is null)
            throw new ResourceNotFoundException($"{nameof(Region)} not found");

        var codeAlreadyExist = await _regionRepository.GetOneByConditionAsync(r => r.Code.Equals(request.Code), cancellationToken);
        if (codeAlreadyExist is not null)
            throw new CodeAlreadyExistException(codeAlreadyExist.Code);

        CountryId? countryId = null;
        if(request.CountryId is not null)
        {
            countryId = CountryId.Of(request.CountryId.Value);
            var country = await countryRepository.GetByIdAsync(countryId, cancellationToken);
            if (country is null)
                throw new ResourceNotFoundException($"{nameof(Country)} with ID {countryId} not found.");
        }

        region.Patch(request.Code, request.Name, request.IsEnabled, countryId);

         _regionRepository.Update(region);
        await _regionRepository.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}
