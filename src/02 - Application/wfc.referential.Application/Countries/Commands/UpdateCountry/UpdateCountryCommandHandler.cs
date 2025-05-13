using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.Countries.Exceptions;
using wfc.referential.Domain.CurrencyAggregate;
using wfc.referential.Domain.MonetaryZoneAggregate;


namespace wfc.referential.Application.Countries.Commands.UpdateCountry;

public class UpdateCountryCommandHandler : ICommandHandler<UpdateCountryCommand, Result<Guid>>
{
    private readonly ICountryRepository _countryRepository;
    private readonly ICurrencyRepository _currencyRepository;

    public UpdateCountryCommandHandler(
        ICountryRepository countryRepository,
        ICurrencyRepository currencyRepository)
    {
        _countryRepository = countryRepository;
        _currencyRepository = currencyRepository;
    }

    public async Task<Result<Guid>> Handle(UpdateCountryCommand request, CancellationToken cancellationToken)
    {

        var country = await _countryRepository.GetByIdAsync(request.CountryId, cancellationToken);
        if (country is null)
            throw new KeyNotFoundException($"Country with ID [{request.CountryId}] not found.");

        // Code Uniqueness check like MonetaryZone 
        var duplicate = await _countryRepository.GetByCodeAsync(request.Code, cancellationToken);
        if (duplicate is not null &&
            !string.Equals(country.Code, duplicate.Code, StringComparison.OrdinalIgnoreCase))
            throw new CountryCodeAlreadyExistException(request.Code);

        // update currencyId if provided as it is an optional field
        CurrencyId? currencyId = null;
        if (request.CurrencyId.HasValue)
        {
            currencyId = new CurrencyId(request.CurrencyId.Value);
        }

        country.Update(
            abbreviation: request.Abbreviation,
            name: request.Name,
            code: request.Code,
            ISO2: request.ISO2,
            ISO3: request.ISO3,
            dialingCode: request.DialingCode,
            timeZone: request.TimeZone,
            hasSector: request.HasSector,
            isSmsEnabled: request.IsSmsEnabled,
            numberDecimalDigits: request.NumberDecimalDigits,
            monetaryZoneId: new MonetaryZoneId(request.MonetaryZoneId),
            currencyId: currencyId,
            isEnabled: request.IsEnabled
        );

        await _countryRepository.UpdateAsync(country, cancellationToken);  

        return Result.Success(country.Id!.Value);

    }
}
