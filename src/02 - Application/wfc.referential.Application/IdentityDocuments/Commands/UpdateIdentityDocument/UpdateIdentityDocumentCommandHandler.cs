using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;

namespace wfc.referential.Application.IdentityDocuments.Commands.UpdateIdentityDocument;
public class UpdateIdentityDocumentCommandHandler(IIdentityDocumentRepository repository, ICacheService cache) : ICommandHandler<UpdateIdentityDocumentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(UpdateIdentityDocumentCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(request.IdentityDocumentId, cancellationToken);
        if (entity is null)
            throw new ResourceNotFoundException("IdentityDocument not found");

        entity.Update(request.Code, request.Name, request.Description, request.IsEnabled);

        await repository.UpdateAsync(entity, cancellationToken);
        await cache.RemoveAsync(request.CacheKey, cancellationToken);

        return Result.Success(entity.Id.Value);
    }
}