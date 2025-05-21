using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;

namespace wfc.referential.Application.IdentityDocuments.Commands.DeleteIdentityDocument;

public class DeleteIdentityDocumentCommandHandler(IIdentityDocumentRepository repository) 
    : ICommandHandler<DeleteIdentityDocumentCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteIdentityDocumentCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(request.IdentityDocumentId, cancellationToken);
        if (entity is null)
            throw new ResourceNotFoundException("IdentityDocument not found");

        entity.SetInactive();
        await repository.UpdateAsync(entity, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}