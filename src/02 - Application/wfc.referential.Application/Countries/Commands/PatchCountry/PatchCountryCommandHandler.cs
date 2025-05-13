using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.Countries.Exceptions;
using wfc.referential.Domain.CurrencyAggregate;
using wfc.referential.Domain.MonetaryZoneAggregate;


namespace wfc.referential.Application.Countries.Commands.PatchCountry;

public class PatchCountryCommandHandler : ICommandHandler<PatchCountryCommand, Result<Guid>>
{
    private readonly ICountryRepository _countryRepo;
    private readonly ICurrencyRepository _currencyRepo;

    public PatchCountryCommandHandler(ICountryRepository countryRepo,
                                      ICurrencyRepository currencyRepo)
    {
        _countryRepo = countryRepo;
        _currencyRepo = currencyRepo;
    }

    public async Task<Result<Guid>> Handle(PatchCountryCommand request, CancellationToken ct)
    {
        var country = await _countryRepo.GetByIdAsync(request.CountryId, ct);
        if (country is null)
            throw new KeyNotFoundException("Country not found");

        if(!string.IsNullOrEmpty(request.Code))
        {
            var isExist = await _countryRepo.GetByCodeAsync(request.Code, ct);
            if (isExist is not null && !string.Equals(isExist.Code, country.Code, StringComparison.OrdinalIgnoreCase))
                throw new CountryCodeAlreadyExistException(request.Code);
        }

        // Map only non‑null properties onto the aggregate
        request.Adapt(country);

        // If MonetaryZoneId is provided, update it
        if (request.MonetaryZoneId.HasValue)
            country.Update(
                country.Abbreviation,
                country.Name,
                country.Code,
                country.ISO2,
                country.ISO3,
                country.DialingCode,
                country.TimeZone,
                country.HasSector,
                country.IsSmsEnabled,
                country.NumberDecimalDigits,
                new MonetaryZoneId(request.MonetaryZoneId.Value),
                country.CurrencyId,
                country.IsEnabled);

        // If CurrencyId provided, fetch currency
        if (request.CurrencyId.HasValue)
        {
            country.Update(
                country.Abbreviation,
                country.Name,
                country.Code,
                country.ISO2,
                country.ISO3,
                country.DialingCode,
                country.TimeZone,
                country.HasSector,
                country.IsSmsEnabled,
                country.NumberDecimalDigits,
                country.MonetaryZoneId,
                new CurrencyId(request.CurrencyId.Value),
                country.IsEnabled);
        }

        // raise CountryPatchedEvent
        country.Patch();                                   
        await _countryRepo.UpdateAsync(country, ct);      

        return Result.Success(country.Id!.Value);
    }
}
