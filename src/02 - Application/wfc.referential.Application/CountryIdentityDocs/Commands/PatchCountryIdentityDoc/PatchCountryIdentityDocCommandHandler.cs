using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.CountryIdentityDocAggregate;
using wfc.referential.Domain.IdentityDocumentAggregate;

namespace wfc.referential.Application.CountryIdentityDocs.Commands.PatchCountryIdentityDoc;

public class PatchCountryIdentityDocCommandHandler : ICommandHandler<PatchCountryIdentityDocCommand, Guid>
{
    private readonly ICountryIdentityDocRepository _repository;
    private readonly ICountryRepository _countryRepository;
    private readonly IIdentityDocumentRepository _identityDocumentRepository;

    public PatchCountryIdentityDocCommandHandler(
        ICountryIdentityDocRepository repository,
        ICountryRepository countryRepository,
        IIdentityDocumentRepository identityDocumentRepository)
    {
        _repository = repository;
        _countryRepository = countryRepository;
        _identityDocumentRepository = identityDocumentRepository;
    }

    public async Task<Guid> Handle(PatchCountryIdentityDocCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.CountryIdentityDocId, cancellationToken);
        if (entity == null)
            throw new BusinessException("CountryIdentityDoc not found");

        // Validation conditionnelle des clés étrangères
        CountryId countryId = entity.CountryId;
        IdentityDocumentId identityDocumentId = entity.IdentityDocumentId;
        bool isEnabled = entity.IsEnabled;

        if (request.CountryId.HasValue)
        {
            var country = await _countryRepository.GetByIdAsync(request.CountryId.Value, cancellationToken);
            if (country is null)
                throw new BusinessException($"Country with ID {request.CountryId} not found");

            countryId = new CountryId(request.CountryId.Value);
        }

        if (request.IdentityDocumentId.HasValue)
        {
            var identityDocument = await _identityDocumentRepository.GetByIdAsync(request.IdentityDocumentId.Value, cancellationToken);
            if (identityDocument is null)
                throw new BusinessException($"IdentityDocument with ID {request.IdentityDocumentId} not found");

            identityDocumentId = IdentityDocumentId.Of(request.IdentityDocumentId.Value);
        }

        if (request.IsEnabled.HasValue)
        {
            isEnabled = request.IsEnabled.Value;
        }

        entity.Update(countryId, identityDocumentId, isEnabled);

        entity.Patch();

        await _repository.UpdateAsync(entity, cancellationToken);

        return entity.Id.Value;
    }
}