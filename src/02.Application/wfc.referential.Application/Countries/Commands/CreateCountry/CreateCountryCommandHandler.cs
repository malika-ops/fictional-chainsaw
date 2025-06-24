using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.Countries.Exceptions;
using wfc.referential.Domain.CurrencyAggregate;
using wfc.referential.Domain.MonetaryZoneAggregate;

namespace wfc.referential.Application.Countries.Commands.CreateCountry;

public class CreateCountryCommandHandler : ICommandHandler<CreateCountryCommand, Result<Guid>>
{
    private readonly ICountryRepository _countryRepository;
    private readonly ICurrencyRepository _currencyRepository;
    private readonly IMonetaryZoneRepository _monetaryZoneRepository;

    public CreateCountryCommandHandler(
        ICountryRepository countryRepository,
        ICurrencyRepository currencyRepository,
        IMonetaryZoneRepository monetaryZoneRepository)
    {
        _countryRepository = countryRepository;
        _currencyRepository = currencyRepository;
        _monetaryZoneRepository = monetaryZoneRepository;
    }

    public async Task<Result<Guid>> Handle(CreateCountryCommand request, CancellationToken cancellationToken)
    {
        // Check for an existing country with the same code to enforce uniqueness.
        var existingCountry = await _countryRepository.GetOneByConditionAsync(c => c.Code == request.Code, cancellationToken);
        if (existingCountry is not null)
            throw new CountryCodeAlreadyExistException(request.Code);

        var countryId = CountryId.Of(Guid.NewGuid());
        var currencyId = CurrencyId.Of(request.CurrencyId);
        var monetaryZoneId = MonetaryZoneId.Of(request.MonetaryZoneId);

        // Validate the existence of the Monetary Zone and Currency
        var monetaryZone = await _monetaryZoneRepository.GetByIdAsync(monetaryZoneId, cancellationToken) ??
            throw new ResourceNotFoundException($"Monetary Zone with Id '{request.MonetaryZoneId}' not found");

        var currency = await _currencyRepository.GetByIdAsync(currencyId, cancellationToken) ??
            throw new ResourceNotFoundException($"Currency with Id '{request.CurrencyId}' not found");

        var newCountry = Country.Create(
            countryId,
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
            currencyId
        );

        await _countryRepository.AddAsync(newCountry, cancellationToken);

        await _countryRepository.SaveChangesAsync(cancellationToken);

        return Result.Success(newCountry.Id!.Value);
    }
}
