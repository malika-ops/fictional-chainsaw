using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CountryServiceAggregate;

namespace wfc.referential.Application.CountryServices.Commands.DeleteCountryService;

public class DeleteCountryServiceCommandHandler(ICountryServiceRepository _countryServiceRepository)
    : ICommandHandler<DeleteCountryServiceCommand, Result<bool>>
{

    public async Task<Result<bool>> Handle(DeleteCountryServiceCommand cmd, CancellationToken ct)
    {
        var countryService = await _countryServiceRepository.GetByIdAsync(CountryServiceId.Of(cmd.CountryServiceId), ct);
        if (countryService is null)
            throw new ResourceNotFoundException($"Country service [{cmd.CountryServiceId}] not found.");

        countryService.Disable();
        await _countryServiceRepository.SaveChangesAsync(ct);

        return Result.Success(true);
    }
}