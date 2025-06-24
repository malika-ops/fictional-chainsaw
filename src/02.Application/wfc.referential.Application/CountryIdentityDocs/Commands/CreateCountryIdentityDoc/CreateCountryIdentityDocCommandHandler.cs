using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.CountryIdentityDocAggregate;
using wfc.referential.Domain.CountryIdentityDocAggregate.Exceptions;
using wfc.referential.Domain.IdentityDocumentAggregate;

namespace wfc.referential.Application.CountryIdentityDocs.Commands.CreateCountryIdentityDoc;

public class CreateCountryIdentityDocCommandHandler : ICommandHandler<CreateCountryIdentityDocCommand, Result<Guid>>
{
    private readonly ICountryIdentityDocRepository _countryIdentityDocRepository;
    private readonly ICountryRepository _countryRepository;
    private readonly IIdentityDocumentRepository _identityDocumentRepository;

    public CreateCountryIdentityDocCommandHandler(
        ICountryIdentityDocRepository countryIdentityDocRepository,
        ICountryRepository countryRepository,
        IIdentityDocumentRepository identityDocumentRepository)
    {
        _countryIdentityDocRepository = countryIdentityDocRepository;
        _countryRepository = countryRepository;
        _identityDocumentRepository = identityDocumentRepository;
    }

    public async Task<Result<Guid>> Handle(CreateCountryIdentityDocCommand command, CancellationToken ct)
    {
        var countryId = CountryId.Of(command.CountryId);
        var identityDocumentId = IdentityDocumentId.Of(command.IdentityDocumentId);

        // Verify country exists
        var country = await _countryRepository.GetByIdAsync(countryId, ct);
        if (country is null)
            throw new ResourceNotFoundException($"Country [{command.CountryId}] not found.");

        // Verify identity document exists
        var identityDocument = await _identityDocumentRepository.GetByIdAsync(identityDocumentId, ct);
        if (identityDocument is null)
            throw new ResourceNotFoundException($"Identity document [{command.IdentityDocumentId}] not found.");

        // Check if association already exists
        var exists = await _countryIdentityDocRepository.GetByConditionAsync(c=>c.CountryId == countryId && c.IdentityDocumentId == identityDocumentId,ct);
        if (exists.Any())
            throw new CountryIdentityDocAlreadyExistsException(command.CountryId, command.IdentityDocumentId);

        var countryIdentityDoc = CountryIdentityDoc.Create(
            CountryIdentityDocId.Of(Guid.NewGuid()),
            countryId,
            identityDocumentId);

        await _countryIdentityDocRepository.AddAsync(countryIdentityDoc, ct);
        await _countryIdentityDocRepository.SaveChangesAsync(ct);

        return Result.Success(countryIdentityDoc.Id!.Value);
    }
}