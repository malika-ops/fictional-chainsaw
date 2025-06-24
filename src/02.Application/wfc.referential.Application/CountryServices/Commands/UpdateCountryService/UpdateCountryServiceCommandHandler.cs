using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.CountryServiceAggregate;
using wfc.referential.Domain.CountryServiceAggregate.Exceptions;
using wfc.referential.Domain.ServiceAggregate;

namespace wfc.referential.Application.CountryServices.Commands.UpdateCountryService;

public class UpdateCountryServiceCommandHandler (
    ICountryServiceRepository _countryServiceRepository,
    ICountryRepository _countryRepository,
    IServiceRepository _serviceRepository
    )
    : ICommandHandler<UpdateCountryServiceCommand, Result<bool>>
{

    public async Task<Result<bool>> Handle(UpdateCountryServiceCommand cmd, CancellationToken ct)
    {
        var countryService = await _countryServiceRepository.GetByIdAsync(CountryServiceId.Of(cmd.CountryServiceId), ct);
        if (countryService is null)
            throw new ResourceNotFoundException($"Country service [{cmd.CountryServiceId}] not found.");

        var countryId = CountryId.Of(cmd.CountryId);
        var serviceId = ServiceId.Of(cmd.ServiceId);

        // Verify country exists
        var country = await _countryRepository.GetByIdAsync(countryId, ct);
        if (country is null)
            throw new ResourceNotFoundException($"Country [{cmd.CountryId}] not found.");

        // Verify service exists
        var service = await _serviceRepository.GetByIdAsync(serviceId, ct);
        if (service is null)
            throw new ResourceNotFoundException($"Service [{cmd.ServiceId}] not found.");

        // Check for duplicate association (if different from current)
        if (countryService.CountryId != countryId || countryService.ServiceId != serviceId)
        {
            var duplicateExists = await _countryServiceRepository.GetByConditionAsync(c => c.CountryId == countryId && c.ServiceId == serviceId, ct);
            if (duplicateExists.Any())
                throw new CountryServiceAlreadyExistsException(cmd.CountryId, cmd.ServiceId);
        }

        countryService.Update(countryId, serviceId, cmd.IsEnabled);

        _countryServiceRepository.Update(countryService);
        await _countryServiceRepository.SaveChangesAsync(ct);

        return Result.Success(true);
    }
}