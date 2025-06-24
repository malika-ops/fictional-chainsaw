using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.CountryServiceAggregate;
using wfc.referential.Domain.CountryServiceAggregate.Exceptions;
using wfc.referential.Domain.ServiceAggregate;

namespace wfc.referential.Application.CountryServices.Commands.CreateCountryService;

public class CreateCountryServiceCommandHandler(
    ICountryServiceRepository _countryServiceRepository,
    ICountryRepository _countryRepository,
    IServiceRepository _serviceRepository
    )
    : ICommandHandler<CreateCountryServiceCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateCountryServiceCommand command, CancellationToken ct)
    {
        var countryId = CountryId.Of(command.CountryId);
        var serviceId = ServiceId.Of(command.ServiceId);

        // Verify country exists
        var country = await _countryRepository.GetByIdAsync(countryId, ct);
        if (country is null)
            throw new ResourceNotFoundException($"Country [{command.CountryId}] not found.");

        // Verify service exists
        var service = await _serviceRepository.GetByIdAsync(serviceId, ct);
        if (service is null)
            throw new ResourceNotFoundException($"Service [{command.ServiceId}] not found.");

        // Check if association already exists
        var exists = await _countryServiceRepository.GetByConditionAsync(c => c.CountryId == countryId && c.ServiceId == serviceId, ct);
        if (exists.Any())
            throw new CountryServiceAlreadyExistsException(command.CountryId, command.ServiceId);

        var countryService = CountryService.Create(
            CountryServiceId.Of(Guid.NewGuid()),
            countryId,
            serviceId);

        await _countryServiceRepository.AddAsync(countryService, ct);
        await _countryServiceRepository.SaveChangesAsync(ct);

        return Result.Success(countryService.Id!.Value);
    }
}