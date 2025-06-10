using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.CountryServiceAggregate;
using wfc.referential.Domain.CountryServiceAggregate.Exceptions;
using wfc.referential.Domain.ServiceAggregate;

namespace wfc.referential.Application.CountryServices.Commands.PatchCountryService;

public class PatchCountryServiceCommandHandler (
    ICountryServiceRepository _countryServiceRepository,
    ICountryRepository _countryRepository,
    IServiceRepository _serviceRepository
    )
    : ICommandHandler<PatchCountryServiceCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(PatchCountryServiceCommand cmd, CancellationToken ct)
    {
        var countryService = await _countryServiceRepository.GetByIdAsync(CountryServiceId.Of(cmd.CountryServiceId), ct);
        if (countryService is null)
            throw new ResourceNotFoundException($"Country service document [{cmd.CountryServiceId}] not found.");

        CountryId? countryId = null;
        ServiceId? serviceId = null;

        // Validate country if provided
        if (cmd.CountryId.HasValue)
        {
            var country = await _countryRepository.GetByIdAsync(cmd.CountryId.Value, ct);
            if (country is null)
                throw new ResourceNotFoundException($"Country [{cmd.CountryId}] not found.");
            countryId = CountryId.Of(cmd.CountryId.Value);
        }

        // Validate service if provided
        if (cmd.ServiceId.HasValue)
        {
            var service = await _serviceRepository.GetByIdAsync(ServiceId.Of(cmd.ServiceId.Value), ct);
            if (service is null)
                throw new ResourceNotFoundException($"service [{cmd.ServiceId}] not found.");
            serviceId = ServiceId.Of(cmd.ServiceId.Value);
        }

        // Check for duplicate association if both IDs are being changed
        if (countryId != null || serviceId != null)
        {
            var finalCountryId = countryId ?? countryService.CountryId;
            var finalserviceId = serviceId ?? countryService.ServiceId;

            if (finalCountryId != countryService.CountryId || finalserviceId != countryService.ServiceId)
            {
                var duplicateExists = await _countryServiceRepository.ExistsByCountryAndServiceAsync(finalCountryId, finalserviceId, ct);
                if (duplicateExists)
                    throw new CountryServiceAlreadyExistsException(finalCountryId.Value, finalserviceId.Value);
            }
        }

        countryService.Patch(countryId, serviceId, cmd.IsEnabled);

        _countryServiceRepository.Update(countryService);
        await _countryServiceRepository.SaveChangesAsync(ct);

        return Result.Success(true);
    }
}