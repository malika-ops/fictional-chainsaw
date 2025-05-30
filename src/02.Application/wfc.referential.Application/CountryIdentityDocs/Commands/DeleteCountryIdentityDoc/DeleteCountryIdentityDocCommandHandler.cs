using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CountryIdentityDocAggregate;

namespace wfc.referential.Application.CountryIdentityDocs.Commands.DeleteCountryIdentityDoc;

public class DeleteCountryIdentityDocCommandHandler : ICommandHandler<DeleteCountryIdentityDocCommand, Result<bool>>
{
    private readonly ICountryIdentityDocRepository _countryIdentityDocRepository;

    public DeleteCountryIdentityDocCommandHandler(ICountryIdentityDocRepository countryIdentityDocRepository)
        => _countryIdentityDocRepository = countryIdentityDocRepository;

    public async Task<Result<bool>> Handle(DeleteCountryIdentityDocCommand cmd, CancellationToken ct)
    {
        var countryIdentityDoc = await _countryIdentityDocRepository.GetByIdAsync(CountryIdentityDocId.Of(cmd.CountryIdentityDocId), ct);
        if (countryIdentityDoc is null)
            throw new ResourceNotFoundException($"Country identity document [{cmd.CountryIdentityDocId}] not found.");

        countryIdentityDoc.Disable();
        await _countryIdentityDocRepository.SaveChangesAsync(ct);

        return Result.Success(true);
    }
}