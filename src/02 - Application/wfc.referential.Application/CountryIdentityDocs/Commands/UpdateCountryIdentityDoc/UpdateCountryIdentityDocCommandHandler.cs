using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.CountryIdentityDocAggregate;
using wfc.referential.Domain.IdentityDocumentAggregate;

namespace wfc.referential.Application.CountryIdentityDocs.Commands.UpdateCountryIdentityDoc;

public class UpdateCountryIdentityDocCommandHandler : ICommandHandler<UpdateCountryIdentityDocCommand, Guid>
{
    private readonly ICountryIdentityDocRepository _repository;
    private readonly ICountryRepository _countryRepository;
    private readonly IIdentityDocumentRepository _identityDocumentRepository;

    public UpdateCountryIdentityDocCommandHandler(
        ICountryIdentityDocRepository repository,
        ICountryRepository countryRepository,
        IIdentityDocumentRepository identityDocumentRepository)
    {
        _repository = repository;
        _countryRepository = countryRepository;
        _identityDocumentRepository = identityDocumentRepository;
    }

    public async Task<Guid> Handle(UpdateCountryIdentityDocCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.CountryIdentityDocId, cancellationToken);
        if (entity is null)
            throw new BusinessException($"CountryIdentityDoc with ID {request.CountryIdentityDocId} not found");

        // Vérifier si le pays existe
        var country = await _countryRepository.GetByIdAsync(request.CountryId, cancellationToken);
        if (country is null)
            throw new BusinessException($"Country with ID {request.CountryId} not found");

        // Vérifier si le document d'identité existe
        var identityDocument = await _identityDocumentRepository.GetByIdAsync(request.IdentityDocumentId, cancellationToken);
        if (identityDocument is null)
            throw new BusinessException($"IdentityDocument with ID {request.IdentityDocumentId} not found");

        var countryId = new CountryId(request.CountryId);
        var identityDocumentId = IdentityDocumentId.Of(request.IdentityDocumentId);

        entity.Update(countryId, identityDocumentId, request.IsEnabled);

        await _repository.UpdateAsync(entity, cancellationToken);

        return entity.Id.Value;
    }
}