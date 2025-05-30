using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.IdentityDocumentAggregate;

namespace wfc.referential.Application.IdentityDocuments.Commands.DeleteIdentityDocument;

public class DeleteIdentityDocumentCommandHandler : ICommandHandler<DeleteIdentityDocumentCommand, Result<bool>>
{
    private readonly IIdentityDocumentRepository _repo;

    public DeleteIdentityDocumentCommandHandler(IIdentityDocumentRepository repo) => _repo = repo;

    public async Task<Result<bool>> Handle(DeleteIdentityDocumentCommand cmd, CancellationToken ct)
    {
        var identityDocument = await _repo.GetByIdAsync(IdentityDocumentId.Of(cmd.IdentityDocumentId), ct);
        if (identityDocument is null)
            throw new ResourceNotFoundException($"Identity document [{cmd.IdentityDocumentId}] not found.");

        identityDocument.Disable();
        await _repo.SaveChangesAsync(ct);
        return Result.Success(true);
    }
}