using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.IdentityDocumentAggregate;
using wfc.referential.Domain.IdentityDocumentAggregate.Exceptions;

namespace wfc.referential.Application.IdentityDocuments.Commands.PatchIdentityDocument;

public class PatchIdentityDocumentCommandHandler : ICommandHandler<PatchIdentityDocumentCommand, Result<bool>>
{
    private readonly IIdentityDocumentRepository _repo;

    public PatchIdentityDocumentCommandHandler(IIdentityDocumentRepository repo)
    {
        _repo = repo;
    }

    public async Task<Result<bool>> Handle(PatchIdentityDocumentCommand cmd, CancellationToken ct)
    {
        var identityDocument = await _repo.GetByIdAsync(IdentityDocumentId.Of(cmd.IdentityDocumentId), ct);
        if (identityDocument is null)
            throw new ResourceNotFoundException($"Identity document [{cmd.IdentityDocumentId}] not found.");

        // duplicate Code check
        if (!string.IsNullOrWhiteSpace(cmd.Code))
        {
            var dup = await _repo.GetOneByConditionAsync(c => c.Code == cmd.Code, ct);
            if (dup is not null && dup.Id != identityDocument.Id)
                throw new IdentityDocumentCodeAlreadyExistException(cmd.Code);
        }

        identityDocument.Patch(
            cmd.Code,
            cmd.Name,
            cmd.Description,
            cmd.IsEnabled);

        _repo.Update(identityDocument);
        await _repo.SaveChangesAsync(ct);

        return Result.Success(true);
    }
}