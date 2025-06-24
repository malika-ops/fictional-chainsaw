using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.MonetaryZoneAggregate.Exceptions;


namespace wfc.referential.Application.Countries.Commands.DeleteCountry;

public class DeleteCountryCommandHandler : ICommandHandler<DeleteCountryCommand, Result<bool>>
{
    private readonly ICountryRepository _countryRepository;
    private readonly IPartnerCountryRepository _partnerCountryRepository;
    private readonly ICorridorRepository _corridorRepository;
    private readonly ICountryIdentityDocRepository _countryIdentityDocRepository;
    private readonly ICountryServiceRepository _countryServiceRepository;
    private readonly IAffiliateRepository _affiliateRepository;

    public DeleteCountryCommandHandler(ICountryRepository countryRepository, 
        IPartnerCountryRepository partnerCountryRepository,
        ICorridorRepository corridorRepository,
        ICountryIdentityDocRepository countryIdentityDocRepository,
        ICountryServiceRepository countryServiceRepository,
        IAffiliateRepository affiliateRepository)
    {
        _countryRepository = countryRepository;
        _partnerCountryRepository = partnerCountryRepository;
        _corridorRepository = corridorRepository;
        _countryIdentityDocRepository = countryIdentityDocRepository;
        _countryServiceRepository = countryServiceRepository;
        _affiliateRepository = affiliateRepository;
    }

    public async Task<Result<bool>> Handle(DeleteCountryCommand request, CancellationToken cancellationToken)
    {
        var countryId = CountryId.Of(request.CountryId);

        var country = await _countryRepository.GetByIdWithIncludesAsync(countryId, cancellationToken, c => c.Regions) 
            ?? throw new ResourceNotFoundException($"Country with ID [{request.CountryId}] not found.");

        // Ensure the country has no associated regions.
        if (country.Regions.Count > 0)
            throw new InvalidDeletingException("Cannot delete Country with existing regions.");

        // Ensure the country is not associated with any partner countries.
        var partnerCountries = await _partnerCountryRepository.GetOneByConditionAsync( c => c.CountryId == countryId, cancellationToken);
        if (partnerCountries is not null)
            throw new InvalidDeletingException("Cannot delete Country that is associated with Partners.");

        // Ensure the country is not associated with any corridors.
        var corridors = await _corridorRepository.GetOneByConditionAsync(c => c.SourceCountryId == countryId ||
         c.DestinationCountryId == countryId, cancellationToken);
        if (corridors is not null)
            throw new InvalidDeletingException("Cannot delete Country that is associated with Corridors.");

        // Ensure the country is not associated with any identity documents.
        var identityDocs = await _countryIdentityDocRepository.GetOneByConditionAsync(c => c.CountryId == countryId, cancellationToken);
        if (identityDocs is not null)
            throw new InvalidDeletingException("Cannot delete Country that is associated with Identity Documents.");

        // Ensure the country is not associated with any services.
        var countryServices = await _countryServiceRepository.GetOneByConditionAsync(c => c.CountryId == countryId, cancellationToken);
        if (countryServices is not null)
            throw new InvalidDeletingException("Cannot delete Country that is associated with Services.");

        // Ensure the country is not associated with any affiliates.
        var affiliates = await _affiliateRepository.GetOneByConditionAsync(a => a.CountryId == countryId, cancellationToken);
        if (affiliates is not null)
            throw new InvalidDeletingException("Cannot delete Country that is associated with Affiliates.");


        // Soft-delete: update the status by calling the Disable method,
        country.Disable();

        await _countryRepository.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}
