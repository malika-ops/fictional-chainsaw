using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.IdentityDocumentAggregate;

namespace wfc.referential.Application.IdentityDocuments.Commands.PatchIdentityDocument;

public class PatchIdentityDocumentCommandHandler(IIdentityDocumentRepository repository, ICacheService cacheService) : ICommandHandler<PatchIdentityDocumentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(PatchIdentityDocumentCommand request, CancellationToken cancellationToken)
    {
        var identitydocument = await repository.GetByIdAsync(request.IdentityDocumentId, cancellationToken);

        if (identitydocument is null)
            throw new ResourceNotFoundException($"{nameof(IdentityDocument)} not found");

        request.Adapt(identitydocument);
        identitydocument.Patch();
        await repository.UpdateAsync(identitydocument, cancellationToken);

        await cacheService.SetAsync(request.CacheKey, identitydocument, TimeSpan.FromMinutes(request.CacheExpiration), cancellationToken);

        return Result.Success(identitydocument.Id!.Value);
    }
}