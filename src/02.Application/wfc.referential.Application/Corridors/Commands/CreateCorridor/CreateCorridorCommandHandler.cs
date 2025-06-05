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
        CityId? sourceCityId = null;
        CityId? destCityId = null;
        CountryId? sourceCountryId = null;
        CountryId? destCountryId = null;
        AgencyId? sourceBranchId = null;
        AgencyId? destBranchId = null;
        // city ids verification
        if (request.SourceCityId.HasValue)
        {
            sourceCityId = CityId.Of(request.SourceCityId.Value);
            var city = await _cityRepository.GetOneByConditionAsync(c => c.Id == sourceCityId, cancellationToken);
            if (city == null) throw new CityNotFoundException(sourceCityId.Value);
        }
        if (request.DestinationCityId.HasValue)
        {
            destCityId = CityId.Of(request.DestinationCityId.Value);
            var city = await _cityRepository.GetOneByConditionAsync(c => c.Id == destCityId, cancellationToken);
            if (city == null) throw new CityNotFoundException(destCityId.Value);
        }
        // country ids verification
        if (request.SourceCountryId.HasValue)
        {
            sourceCountryId = CountryId.Of(request.SourceCountryId.Value);
            var country = await _countryRepository.GetByIdAsync(sourceCountryId.Value, cancellationToken);
            if (country == null) throw new ResourceNotFoundException(sourceCountryId.Value.ToString());
        }
        if (request.DestinationCountryId.HasValue)
        {
            destCountryId = CountryId.Of(request.DestinationCountryId.Value);
            var country = await _countryRepository.GetByIdAsync(destCountryId.Value, cancellationToken);
            if (country == null) throw new ResourceNotFoundException(destCountryId.Value.ToString());
        }
        // agency ids verification
        if (request.SourceBranchId.HasValue)
        {
            sourceBranchId = AgencyId.Of(request.SourceBranchId.Value);
            var branch = await _agencyRepository.GetOneByConditionAsync(b => b.Id  == sourceBranchId, cancellationToken);
            if (branch == null) throw new ResourceNotFoundException(sourceBranchId.Value.ToString());
        }
        if (request.DestinationBranchId.HasValue)
        {
            destBranchId = AgencyId.Of(request.DestinationBranchId.Value);
            var branch = await _agencyRepository.GetOneByConditionAsync(b => b.Id == destBranchId, cancellationToken);
            if (branch == null) throw new ResourceNotFoundException(destBranchId.Value.ToString());
        }


        var corridor = Corridor.Create(corridorId,
            sourceCountryId,
            destCountryId,
            sourceCityId, 
            destCityId,
            sourceBranchId,
            destBranchId);

        await corridorRepository.AddAsync(corridor, cancellationToken);
        await corridorRepository.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveByPrefixAsync(CacheKeys.Corridor.Prefix, cancellationToken);

        return Result.Success(corridor.Id!.Value);
    }
}
