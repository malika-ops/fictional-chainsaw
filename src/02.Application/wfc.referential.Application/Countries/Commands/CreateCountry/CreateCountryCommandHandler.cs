using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.Countries.Exceptions;
using wfc.referential.Domain.CurrencyAggregate;
using wfc.referential.Domain.MonetaryZoneAggregate;

namespace wfc.referential.Application.Countries.Commands.CreateCountry
{
    public class CreateCountryCommandHandler : ICommandHandler<CreateCountryCommand, Result<Guid>>
    {
        private readonly ICountryRepository _countryRepository;
        private readonly ICurrencyRepository _currencyRepository; 

        public CreateCountryCommandHandler(
            ICountryRepository countryRepository,
            ICurrencyRepository currencyRepository)
        {
            _countryRepository = countryRepository;
            _currencyRepository = currencyRepository;
        }

        public async Task<Result<Guid>> Handle(CreateCountryCommand request, CancellationToken cancellationToken)
        {
            // Check for an existing country with the same code to enforce uniqueness.
            var existingCountry = await _countryRepository.GetByCodeAsync(request.Code, cancellationToken);
            if (existingCountry is not null)
                throw new CountryCodeAlreadyExistException(request.Code);


            // Use a static factory method on Country to create a new instance.
            // This method will internally raise a CountryCreatedEvent.
            var countryId = CountryId.Of(Guid.NewGuid());
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
                request.IsSmsEnabled ?? false,
                request.NumberDecimalDigits,
                true,
                new MonetaryZoneId(request.MonetaryZoneId),
                new CurrencyId(request.CurrencyId)
            );

            await _countryRepository.AddAsync(newCountry, cancellationToken);
            return Result.Success(newCountry.Id!.Value);
        }
    }
}
