using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Constants;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.CityAggregate.Exceptions;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.Countries;

namespace wfc.referential.Application.Corridors.Commands.CreateCorridor;

public class CreateCorridorCommandHandler(ICorridorRepository corridorRepository, ICacheService _cacheService,
    ICityRepository _cityRepository, ICountryRepository _countryRepository, IAgencyRepository _agencyRepository) 
    : ICommandHandler<CreateCorridorCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateCorridorCommand request, CancellationToken cancellationToken)
    {

        var corridorId = CorridorId.Of(Guid.NewGuid());
        // city ids verification
        if (request.SourceCityId.HasValue)
        {
            var cityId = CityId.Of(request.SourceCityId.Value);
            var city = await _cityRepository.GetOneByConditionAsync(c => c.Id == cityId, cancellationToken);
            if (city == null) throw new CityNotFoundException(cityId.Value);
        }
        if (request.DestinationCityId.HasValue)
        {
            var cityId = CityId.Of(request.DestinationCityId.Value);
            var city = await _cityRepository.GetOneByConditionAsync(c => c.Id == cityId, cancellationToken);
            if (city == null) throw new CityNotFoundException(cityId.Value);
        }
        // country ids verification
        if (request.SourceCountryId.HasValue)
        {
            var countryId = CountryId.Of(request.SourceCountryId.Value);
            var country = await _countryRepository.GetByIdAsync(countryId.Value, cancellationToken);
            if (country == null) throw new ResourceNotFoundException(countryId.Value.ToString());
        }
        if (request.DestinationCountryId.HasValue)
        {
            var countryId = CityId.Of(request.DestinationCountryId.Value);
            var country = await _countryRepository.GetByIdAsync(countryId.Value, cancellationToken);
            if (country == null) throw new ResourceNotFoundException(countryId.Value.ToString());
        }
        // agency ids verification
        if (request.SourceBranchId.HasValue)
        {
            var branchId = AgencyId.Of(request.SourceBranchId.Value);
            var branch = await _agencyRepository.GetOneByConditionAsync(b => b.Id  == branchId, cancellationToken);
            if (branch == null) throw new ResourceNotFoundException(branchId.Value.ToString());
        }
        if (request.DestinationBranchId.HasValue)
        {
            var branchId = AgencyId.Of(request.DestinationBranchId.Value);
            var branch = await _agencyRepository.GetOneByConditionAsync(b => b.Id == branchId, cancellationToken);
            if (branch == null) throw new ResourceNotFoundException(branchId.Value.ToString());
        }


        var corridor = Corridor.Create(corridorId,
            request .SourceCountryId.HasValue ? CountryId.Of(request.SourceCountryId.Value) : null,
            request.DestinationCountryId.HasValue ? CountryId.Of(request.DestinationCountryId.Value) : null,
            request .SourceCityId.HasValue ? CityId.Of(request.SourceCityId.Value) : null, 
            request .DestinationCityId.HasValue ? CityId.Of(request.DestinationCityId.Value) : null,
            request .SourceBranchId.HasValue ? AgencyId.Of(request.SourceBranchId.Value) : null,
            request .DestinationBranchId.HasValue ? AgencyId.Of(request.DestinationBranchId.Value) : null);

        await corridorRepository.AddAsync(corridor, cancellationToken);
        await corridorRepository.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveByPrefixAsync(CacheKeys.Corridor.Prefix, cancellationToken);

        return Result.Success(corridor.Id!.Value);
    }
}
