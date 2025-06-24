using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.Countries.Exceptions;
using wfc.referential.Domain.CurrencyAggregate;
using wfc.referential.Domain.MonetaryZoneAggregate;


namespace wfc.referential.Application.Countries.Commands.UpdateCountry;

public class UpdateCountryCommandHandler : ICommandHandler<UpdateCountryCommand, Result<bool>>
{
    private readonly ICountryRepository _countryRepository;
    private readonly ICurrencyRepository _currencyRepository;
    private readonly IMonetaryZoneRepository _monetaryZoneRepository;

    public UpdateCountryCommandHandler(
        ICountryRepository countryRepository,
        ICurrencyRepository currencyRepository,
        IMonetaryZoneRepository monetaryZoneRepository)
    {
        _countryRepository = countryRepository;
        _currencyRepository = currencyRepository;
        _monetaryZoneRepository = monetaryZoneRepository;
    }

    public async Task<Result<bool>> Handle(UpdateCountryCommand request, CancellationToken cancellationToken)
    {
        var countryId = CountryId.Of(request.CountryId);
        var currencyId = CurrencyId.Of(request.CurrencyId);
        var monetaryZoneId = MonetaryZoneId.Of(request.MonetaryZoneId);

        var country = await _countryRepository.GetByIdAsync(countryId, cancellationToken) 
            ?? throw new KeyNotFoundException($"Country with ID [{request.CountryId}] not found.");

        // Code Uniqueness check like MonetaryZone 
        var duplicate = await _countryRepository.GetOneByConditionAsync(c => c.Code == request.Code, cancellationToken);
        if (duplicate is not null &&
            !string.Equals(country.Code, duplicate.Code, StringComparison.OrdinalIgnoreCase))
            throw new CountryCodeAlreadyExistException(request.Code);

        // Validate the existence of the Monetary Zone and Currency
        var monetaryZone = await _monetaryZoneRepository.GetByIdAsync(monetaryZoneId, cancellationToken) ??
            throw new ResourceNotFoundException($"Monetary Zone with Id '{request.MonetaryZoneId}' not found");

        var currency = await _currencyRepository.GetByIdAsync(currencyId, cancellationToken) ??
            throw new ResourceNotFoundException($"Currency with Id '{request.CurrencyId}' not found");

        country.Update(
            request.Abbreviation,
            request.Name,
            request.Code,
            request.ISO2,
            request.ISO3,
            request.DialingCode,
            request.TimeZone,
            request.HasSector,
            isSmsEnabled: request.IsSmsEnabled,
            request.NumberDecimalDigits,
            monetaryZoneId,
            currencyId,
            request.IsEnabled
        );

        await _countryRepository.SaveChangesAsync(cancellationToken);

        return Result.Success(true);

    }
}
