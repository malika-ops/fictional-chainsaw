using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.CountryIdentityDocAggregate;
using wfc.referential.Domain.CountryIdentityDocAggregate.Exceptions;
using wfc.referential.Domain.IdentityDocumentAggregate;

namespace wfc.referential.Application.CountryIdentityDocs.Commands.UpdateCountryIdentityDoc;

public class UpdateCountryIdentityDocCommandHandler : ICommandHandler<UpdateCountryIdentityDocCommand, Result<bool>>
{
    private readonly ICountryIdentityDocRepository _countryIdentityDocRepository;
    private readonly ICountryRepository _countryRepository;
    private readonly IIdentityDocumentRepository _identityDocumentRepository;

    public UpdateCountryIdentityDocCommandHandler(
        ICountryIdentityDocRepository countryIdentityDocRepository,
        ICountryRepository countryRepository,
        IIdentityDocumentRepository identityDocumentRepository)
    {
        _countryIdentityDocRepository = countryIdentityDocRepository;
        _countryRepository = countryRepository;
        _identityDocumentRepository = identityDocumentRepository;
    }

    public async Task<Result<bool>> Handle(UpdateCountryIdentityDocCommand cmd, CancellationToken ct)
    {
        var countryIdentityDoc = await _countryIdentityDocRepository.GetByIdAsync(CountryIdentityDocId.Of(cmd.CountryIdentityDocId), ct);
        if (countryIdentityDoc is null)
            throw new ResourceNotFoundException($"Country identity document [{cmd.CountryIdentityDocId}] not found.");

        var countryId = CountryId.Of(cmd.CountryId);
        var identityDocumentId = IdentityDocumentId.Of(cmd.IdentityDocumentId);

        // Verify country exists
        var country = await _countryRepository.GetByIdAsync(cmd.CountryId, ct);
        if (country is null)
            throw new ResourceNotFoundException($"Country [{cmd.CountryId}] not found.");

        // Verify identity document exists
        var identityDocument = await _identityDocumentRepository.GetByIdAsync(identityDocumentId, ct);
        if (identityDocument is null)
            throw new ResourceNotFoundException($"Identity document [{cmd.IdentityDocumentId}] not found.");

        // Check for duplicate association (if different from current)
        if (countryIdentityDoc.CountryId != countryId || countryIdentityDoc.IdentityDocumentId != identityDocumentId)
        {
            var duplicateExists = await _countryIdentityDocRepository.ExistsByCountryAndIdentityDocumentAsync(countryId, identityDocumentId, ct);
            if (duplicateExists)
                throw new CountryIdentityDocAlreadyExistsException(cmd.CountryId, cmd.IdentityDocumentId);
        }

        countryIdentityDoc.Update(countryId, identityDocumentId, cmd.IsEnabled);

        _countryIdentityDocRepository.Update(countryIdentityDoc);
        await _countryIdentityDocRepository.SaveChangesAsync(ct);

        return Result.Success(true);
    }
}