using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.Countries.Exceptions;
using wfc.referential.Domain.CurrencyAggregate;
using wfc.referential.Domain.MonetaryZoneAggregate;


namespace wfc.referential.Application.Countries.Commands.PatchCountry;

public class PatchCountryCommandHandler : ICommandHandler<PatchCountryCommand, Result<bool>>
{
    private readonly ICountryRepository _countryRepo;
    private readonly ICurrencyRepository _currencyRepo;
    private readonly IMonetaryZoneRepository _monetaryZoneRepository;


    public PatchCountryCommandHandler(ICountryRepository countryRepo,
                                      ICurrencyRepository currencyRepo,
                                      IMonetaryZoneRepository monetaryZoneRepository)
    {
        _countryRepo = countryRepo;
        _currencyRepo = currencyRepo;
        _monetaryZoneRepository = monetaryZoneRepository;
    }

    public async Task<Result<bool>> Handle(PatchCountryCommand request, CancellationToken ct)
    {
        var countryId = CountryId.Of(request.CountryId);
        CurrencyId? currencyId = null;
        MonetaryZoneId? monetaryZoneId = null;

        var country = await _countryRepo.GetByIdAsync(countryId, ct) 
            ?? throw new ResourceNotFoundException("Country not found");

        if (!string.IsNullOrEmpty(request.Code))
        {
            var isExist = await _countryRepo.GetOneByConditionAsync(c => c.Code == request.Code, ct);
            if (isExist is not null && !string.Equals(isExist.Code, country.Code, StringComparison.OrdinalIgnoreCase))
                throw new CountryCodeAlreadyExistException(request.Code);
        }

        // Validate the existence of the Monetary Zone and Currency

        if (request.CurrencyId.HasValue)
        {
            currencyId = CurrencyId.Of(request.CurrencyId.Value);
            var currency = await _currencyRepo.GetByIdAsync(currencyId, ct) 
                ?? throw new ResourceNotFoundException($"Currency with Id '{request.CurrencyId}' not found");
        }
        if(request.MonetaryZoneId.HasValue)
        {
            monetaryZoneId = MonetaryZoneId.Of(request.MonetaryZoneId.Value);
            var monetaryZone = await _monetaryZoneRepository.GetByIdAsync(monetaryZoneId, ct) 
                ?? throw new ResourceNotFoundException($"Monetary Zone with Id '{request.MonetaryZoneId}' not found");
        }

        country.Patch(
                request.Abbreviation,
                request.Name,
                request.Code,
                request.ISO2,
                request.ISO3,
                request.DialingCode,
                request.TimeZone,
                request.HasSector,
                request.IsSmsEnabled,
                request.NumberDecimalDigits,
                monetaryZoneId,
                currencyId,
                request.IsEnabled);

        await _countryRepo.SaveChangesAsync(ct);

        return Result.Success(true);
    }
}
