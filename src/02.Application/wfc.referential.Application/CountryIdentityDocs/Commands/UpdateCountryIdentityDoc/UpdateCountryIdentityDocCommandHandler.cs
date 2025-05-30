using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using BuildingBlocks.Infrastructure.CachingManagement;
using wfc.referential.Application.Constants;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.IdentityDocumentAggregate;

namespace wfc.referential.Application.CountryIdentityDocs.Commands.UpdateCountryIdentityDoc;

public class UpdateCountryIdentityDocCommandHandler(ICountryIdentityDocRepository _repository, ICountryRepository _countryRepository,
    IIdentityDocumentRepository _identityDocumentRepository,CacheService _cacheService) : ICommandHandler<UpdateCountryIdentityDocCommand, Guid>
{
    public async Task<Guid> Handle(UpdateCountryIdentityDocCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.CountryIdentityDocId, cancellationToken);
        if (entity is null)
            throw new BusinessException($"CountryIdentityDoc with ID {request.CountryIdentityDocId} not found");

        var country = await _countryRepository.GetByIdAsync(request.CountryId, cancellationToken);
        if (country is null)
            throw new BusinessException($"Country with ID {request.CountryId} not found");

        var identityDocument = await _identityDocumentRepository.GetByIdAsync(IdentityDocumentId.Of(request.IdentityDocumentId), cancellationToken);
        if (identityDocument is null)
            throw new BusinessException($"IdentityDocument with ID {request.IdentityDocumentId} not found");

        var countryId = CountryId.Of(request.CountryId);
        var identityDocumentId = IdentityDocumentId.Of(request.IdentityDocumentId);

        entity.Update(countryId, identityDocumentId, request.IsEnabled);

        await _repository.UpdateAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveByPrefixAsync(CacheKeys.CountryIdentityDocument.Prefix, cancellationToken);

        return entity.Id!.Value;
    }
}