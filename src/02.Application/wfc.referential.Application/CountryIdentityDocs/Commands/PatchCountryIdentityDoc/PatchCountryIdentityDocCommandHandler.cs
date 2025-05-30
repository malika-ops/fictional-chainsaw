using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Constants;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.CountryIdentityDocAggregate;
using wfc.referential.Domain.IdentityDocumentAggregate;

namespace wfc.referential.Application.CountryIdentityDocs.Commands.PatchCountryIdentityDoc;

public class PatchCountryIdentityDocCommandHandler(ICountryIdentityDocRepository _repository, ICountryRepository _countryRepository,
    IIdentityDocumentRepository _identityDocumentRepository , ICacheService _cacheService) : ICommandHandler<PatchCountryIdentityDocCommand, Guid>
{
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
            var identityDocument = await _identityDocumentRepository.GetByIdAsync(IdentityDocumentId.Of(request.IdentityDocumentId.Value), cancellationToken);
            if (identityDocument is null)
                throw new BusinessException($"IdentityDocument with ID {request.IdentityDocumentId} not found");

            identityDocumentId = IdentityDocumentId.Of(request.IdentityDocumentId.Value);
        }

        if (request.IsEnabled.HasValue)
        {
            isEnabled = request.IsEnabled.Value;
        }

        entity.Patch(countryId, identityDocumentId, isEnabled);

        await _repository.UpdateAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveByPrefixAsync(CacheKeys.CountryIdentityDocument.Prefix, cancellationToken);

        return entity.Id!.Value;
    }
}