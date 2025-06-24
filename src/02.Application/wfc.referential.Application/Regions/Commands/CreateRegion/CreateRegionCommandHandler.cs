using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.RegionAggregate;
using wfc.referential.Domain.RegionAggregate.Exceptions;

namespace wfc.referential.Application.Regions.Commands.CreateRegion;

public class CreateRegionCommandHandler(IRegionRepository regionRepository, ICountryRepository countryRepository) 
    : ICommandHandler<CreateRegionCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateRegionCommand request, CancellationToken cancellationToken)
    {
        var isExist = await regionRepository.GetOneByConditionAsync(r => r.Code == request.Code,cancellationToken);
        if (isExist is not null) throw new CodeAlreadyExistException(request.Code);

        var regionId = RegionId.Create();

        var countryId = CountryId.Of(request.CountryId);
        var country = await countryRepository.GetByIdAsync(countryId, cancellationToken);
        if (country is null)
            throw new ResourceNotFoundException($"{nameof(Country)} with ID {request.CountryId} not found.");

        var region = Region.Create(regionId, request.Code, request.Name, countryId);

        await regionRepository.AddAsync(region, cancellationToken);
        await regionRepository.SaveChangesAsync(cancellationToken);

        return Result.Success(region.Id!.Value);
    }
}
