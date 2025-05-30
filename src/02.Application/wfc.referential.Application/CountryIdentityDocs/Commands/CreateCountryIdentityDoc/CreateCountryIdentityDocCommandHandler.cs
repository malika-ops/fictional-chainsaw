using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Constants;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.CountryIdentityDocAggregate;
using wfc.referential.Domain.CountryIdentityDocAggregate.Exceptions;
using wfc.referential.Domain.IdentityDocumentAggregate;

namespace wfc.referential.Application.CountryIdentityDocs.Commands.CreateCountryIdentityDoc;

public class CreateCountryIdentityDocCommandHandler(ICountryIdentityDocRepository _repository, ICountryRepository _countryRepository,
    IIdentityDocumentRepository _identityDocumentRepository,ICacheService _cacheService) 
    : ICommandHandler<CreateCountryIdentityDocCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateCountryIdentityDocCommand request, CancellationToken cancellationToken)
    {
        var countryId = new CountryId(request.CountryId);
        var identityDocumentId = IdentityDocumentId.Of(request.IdentityDocumentId);

        // Vérifier si le pays existe
        var country = await _countryRepository.GetByIdAsync(request.CountryId, cancellationToken);
        if (country is null)
            throw new BusinessException($"Country with ID {request.CountryId} not found");

        // Vérifier si le document d'identité existe
        var identityDocument = await _identityDocumentRepository.GetByIdAsync(IdentityDocumentId.Of(request.IdentityDocumentId), cancellationToken);
        if (identityDocument is null)
            throw new BusinessException($"IdentityDocument with ID {request.IdentityDocumentId} not found");

        // Vérifier si l'association existe déjà
        var exists = await _repository.ExistsByCountryAndIdentityDocumentAsync(countryId, identityDocumentId, cancellationToken);
        if (exists)
            throw new CountryIdentityDocAlreadyExistsException(request.CountryId, request.IdentityDocumentId);

        var id = CountryIdentityDocId.Of(Guid.NewGuid());
        var entity = CountryIdentityDoc.Create(
            id,
            countryId,
            identityDocumentId,
            true);

        await _repository.AddAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveByPrefixAsync(CacheKeys.CountryIdentityDocument.Prefix, cancellationToken);

        return Result.Success(entity.Id!.Value);
    }
}