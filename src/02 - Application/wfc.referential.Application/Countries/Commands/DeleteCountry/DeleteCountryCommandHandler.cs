using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.MonetaryZoneAggregate.Exceptions;


namespace wfc.referential.Application.Countries.Commands.DeleteCountry;

public class DeleteCountryCommandHandler : ICommandHandler<DeleteCountryCommand, Result<bool>>
{
    private readonly ICountryRepository _countryRepository;

    public DeleteCountryCommandHandler(ICountryRepository countryRepository)
    {
        _countryRepository = countryRepository;
    }

    public async Task<Result<bool>> Handle(DeleteCountryCommand request, CancellationToken cancellationToken)
    {

        // Retrieve the country using its Guid.
        var country = await _countryRepository.GetByIdAsync(request.CountryId, cancellationToken);
        if (country is null)
            throw new BusinessException($"Country with ID [{request.CountryId}] not found.");

        // Ensure the country has no associated regions.
        if (country.Regions.Count > 0)
            throw new InvalidDeletingException("Cannot delete Country with existing regions.");

        // Soft-delete: update the status by calling the Disable method,
        // which raises a domain event.
        country.Disable();

        // Persist the status update.
        await _countryRepository.UpdateAsync(country, cancellationToken);

        return Result.Success(true);
    }
}
