using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using wfc.referential.Application.Constants;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CountryIdentityDocAggregate.Exceptions;

namespace wfc.referential.Application.CountryIdentityDocs.Commands.DeleteCountryIdentityDoc;

public class DeleteCountryIdentityDocCommandHandler(ICountryIdentityDocRepository _repository,ICacheService _cacheService) : ICommandHandler<DeleteCountryIdentityDocCommand, bool>
{
    public async Task<bool> Handle(DeleteCountryIdentityDocCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.CountryIdentityDocId, cancellationToken);
        if (entity == null)
            throw new CountryIdentityDocException("CountryIdentityDoc not found");

        entity.Disable();

        await _repository.UpdateAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        // Clear cache
        await _cacheService.RemoveByPrefixAsync(CacheKeys.CountryIdentityDocument.Prefix, cancellationToken);

        return true;
    }
}