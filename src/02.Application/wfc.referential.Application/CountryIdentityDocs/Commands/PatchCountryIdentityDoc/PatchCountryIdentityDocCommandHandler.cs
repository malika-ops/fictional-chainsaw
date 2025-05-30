using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.CountryIdentityDocAggregate;
using wfc.referential.Domain.CountryIdentityDocAggregate.Exceptions;
using wfc.referential.Domain.IdentityDocumentAggregate;

namespace wfc.referential.Application.CountryIdentityDocs.Commands.PatchCountryIdentityDoc;

public class PatchCountryIdentityDocCommandHandler : ICommandHandler<PatchCountryIdentityDocCommand, Result<bool>>
{
    private readonly ICountryIdentityDocRepository _countryIdentityDocRepository;
    private readonly ICountryRepository _countryRepository;
    private readonly IIdentityDocumentRepository _identityDocumentRepository;

    public PatchCountryIdentityDocCommandHandler(
        ICountryIdentityDocRepository countryIdentityDocRepository,
        ICountryRepository countryRepository,
        IIdentityDocumentRepository identityDocumentRepository)
    {
        _countryIdentityDocRepository = countryIdentityDocRepository;
        _countryRepository = countryRepository;
        _identityDocumentRepository = identityDocumentRepository;
    }

    public async Task<Result<bool>> Handle(PatchCountryIdentityDocCommand cmd, CancellationToken ct)
    {
        var countryIdentityDoc = await _countryIdentityDocRepository.GetByIdAsync(CountryIdentityDocId.Of(cmd.CountryIdentityDocId), ct);
        if (countryIdentityDoc is null)
            throw new ResourceNotFoundException($"Country identity document [{cmd.CountryIdentityDocId}] not found.");

        CountryId? countryId = null;
        IdentityDocumentId? identityDocumentId = null;

        // Validate country if provided
        if (cmd.CountryId.HasValue)
        {
            var country = await _countryRepository.GetByIdAsync(cmd.CountryId.Value, ct);
            if (country is null)
                throw new ResourceNotFoundException($"Country [{cmd.CountryId}] not found.");
            countryId = CountryId.Of(cmd.CountryId.Value);
        }

        // Validate identity document if provided
        if (cmd.IdentityDocumentId.HasValue)
        {
            var identityDocument = await _identityDocumentRepository.GetByIdAsync(IdentityDocumentId.Of(cmd.IdentityDocumentId.Value), ct);
            if (identityDocument is null)
                throw new ResourceNotFoundException($"Identity document [{cmd.IdentityDocumentId}] not found.");
            identityDocumentId = IdentityDocumentId.Of(cmd.IdentityDocumentId.Value);
        }

        // Check for duplicate association if both IDs are being changed
        if (countryId != null || identityDocumentId != null)
        {
            var finalCountryId = countryId ?? countryIdentityDoc.CountryId;
            var finalIdentityDocumentId = identityDocumentId ?? countryIdentityDoc.IdentityDocumentId;

            if (finalCountryId != countryIdentityDoc.CountryId || finalIdentityDocumentId != countryIdentityDoc.IdentityDocumentId)
            {
                var duplicateExists = await _countryIdentityDocRepository.ExistsByCountryAndIdentityDocumentAsync(finalCountryId, finalIdentityDocumentId, ct);
                if (duplicateExists)
                    throw new CountryIdentityDocAlreadyExistsException(finalCountryId.Value, finalIdentityDocumentId.Value);
            }
        }

        countryIdentityDoc.Patch(countryId, identityDocumentId, cmd.IsEnabled);

        _countryIdentityDocRepository.Update(countryIdentityDoc);
        await _countryIdentityDocRepository.SaveChangesAsync(ct);

        return Result.Success(true);
    }
}