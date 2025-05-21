using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.IdentityDocumentAggregate;

namespace wfc.referential.Application.IdentityDocuments.Commands.PatchIdentityDocument;

public class PatchIdentityDocumentCommandHandler(IIdentityDocumentRepository repository) 
    : ICommandHandler<PatchIdentityDocumentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(PatchIdentityDocumentCommand request, CancellationToken cancellationToken)
    {
        var identitydocument = await repository.GetByIdAsync(request.IdentityDocumentId, cancellationToken);

        if (identitydocument is null)
            throw new ResourceNotFoundException($"{nameof(IdentityDocument)} not found");

        request.Adapt(identitydocument);
        identitydocument.Patch(request.Code,request.Name,request.Description,request.IsEnabled);

        await repository.UpdateAsync(identitydocument, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return Result.Success(identitydocument.Id!.Value);
    }
}